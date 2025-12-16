using AutoMapper;
using DataAccessLayer.Data;
using BusinessLayer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

public class IndividualService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    // Constructor with dependency injection
    public IndividualService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx; // Database context
        _mapper = mapper; // Mapping profile for DTOs
    }

    // Get single individual with full details
    public IndividualFullDTO? FullById(string iconst)
    {
        var individual = _ctx.Individuals.FirstOrDefault(i => i.Iconst == iconst);
        return individual == null ? null : _mapper.Map<IndividualFullDTO>(individual);
    }

    // Get individual reference (lightweight)
    public IndividualReferenceDTO? ReferenceByID(string iconst)
    {
        var individual = _ctx.Individuals
            .Include(i => i.IndividualPage)
            .FirstOrDefault(i => i.Iconst == iconst);
        return individual == null ? null : _mapper.Map<IndividualReferenceDTO>(individual);
    }

    // Get multiple individuals (for lists)
    // FIX: Change to "keyset paging"
    public List<IndividualReferenceDTO> ReferenceByPage(int page = 1, int pageSize = 20)
    {
        var individuals = _ctx.Individuals
            .OrderByDescending(t => t.NameRating) // WARN: Might cause an issue if nulls are treated as lowest (We have a lot of those in the db)
            .Include(i => i.IndividualPage)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return _mapper.Map<List<IndividualReferenceDTO>>(individuals);
    }

    // Get most popular individuals, excluding null ratings, paginated
    public List<IndividualReferenceDTO> GetMostPopularIndividuals(int page = 1, int pageSize = 20)
    {
        var individuals = _ctx.Individuals
            .Where(i => i.NameRating != null && i.BirthYear != null)
            .OrderByDescending(i => i.NameRating)
            .ThenBy(i => i.Iconst) // Secondary sort for consistent ordering when ratings are equal
            .Include(i => i.IndividualPage)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return _mapper.Map<List<IndividualReferenceDTO>>(individuals);
    }

    // Get most popular individuals ordered by number of votes
    public List<IndividualReferenceWithTotalVotesDTO> GetMostPopularIndividualsByVotes(int page = 1, int pageSize = 20)
    {
        var offset = (page - 1) * pageSize;

        // Although a materialized view could be used here for performance, we are using regular SQL since this was the fastest way of implementing function that is seldom used.
        // Arguably, it would also be an idea to at least have this as a function in the database, but again, this is seldom used enough to not warrant it
        // Finally, it might be an idea to index some of the columns used in the query for performance, but again, this is seldom used enough to not warrant it
        //
        // NOTE: This query was obviously optimized for performance, which was done with relatively extensive help from copilot.
        // The query first computes the total votes for each actor/actress in movies, orders them by total votes, and limits to the top 1000.
        // Then, it joins this result with the individual and page tables to get the required details.
        // This approach separates the work into two steps, allowing each part to be optimized individually.
        var individuals = _ctx.Database
            .SqlQueryRaw<IndividualReferenceWithTotalVotesDTO>(
                @"WITH popular_actors AS (
                    SELECT 
                        c.iconst,
                        SUM(t.numvotes) AS total_votes
                    FROM mdb.contributor c
                    INNER JOIN mdb.title t USING (tconst)
                    WHERE c.contribution IN ('actor', 'actress')
                      AND t.media_type = 'movie'
                      AND t.numvotes > 0
                    GROUP BY c.iconst
                    ORDER BY total_votes DESC
                    LIMIT 1000
                )
                SELECT 
                    pa.iconst AS ""Id"",
                    i.name AS ""Name"",
                    p.pconst AS ""PageId"",
                    pa.total_votes AS ""TotalVotes""
                FROM popular_actors pa
                INNER JOIN mdb.individual i USING (iconst)
                LEFT JOIN mdb.page p USING (iconst)
                ORDER BY pa.total_votes DESC
                LIMIT {0} OFFSET {1}",
                pageSize, offset)
            .AsNoTracking() // no tracking for read-only query which improves performance slightly (im told)
            .ToList();

        return individuals;
    }

    // Get titles associated with an individual
    public List<TitlePreviewDTO> TitlesByIndividual(string iconst)
    {
        var titles = _ctx.Titles
            .Where(t => t.Contributors.Any(c => c.Iconst == iconst))
            .Include(t => t.TitlePage)
            .ToList();
        return _mapper.Map<List<TitlePreviewDTO>>(titles);
    }

    // Get popular actors related to a given const (iconst or tconst)
    public List<IndividualFullDTO> GetPopularActors(string qConst)
    {
        var individuals = _ctx.Individuals
            .FromSqlRaw("SELECT * FROM mdb.popular_actor({0})", qConst)
            .ToList();
        return _mapper.Map<List<IndividualFullDTO>>(individuals);
    }

    // Find co-actors for a given actor name (calls mdb.find_co_actors)
    public List<CoActorDTO> FindCoActors(string actorName)
    {
        Console.WriteLine($"Finding co-actors for: {actorName}");
        // decode URL-encoded name
        actorName = System.Net.WebUtility.UrlDecode(actorName);
        Console.WriteLine($"Encoded actor name: {actorName}");

        if (string.IsNullOrWhiteSpace(actorName))
            return new List<CoActorDTO>();

        // Use parameterized raw SQL to avoid injection while letting PostgreSQL execute the function
        var results = _ctx.Database
            .SqlQueryRaw<CoActorDTO>("SELECT iconst AS Id, primaryname AS Name, co_count AS CollaborationCount FROM mdb.find_co_actors({0})", actorName)
            .ToList();

        return results;
    }

    // Search individuals
    public List<IndividualSearchResultDTO> SearchIndividuals(string name)
    {
        var results = _ctx.Database
            .SqlQueryRaw<IndividualSearchResultDTO>("SELECT iconst AS Id, name AS Name, contribution AS Contribution, title_name AS TitleName, COALESCE(detail, '') AS Detail, COALESCE(genre, '') AS Genre FROM mdb.find_name({0})", name)
            .ToList();
        return results;
    }
}
