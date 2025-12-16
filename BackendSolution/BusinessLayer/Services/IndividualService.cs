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
        // INFO: This query aggregates total votes for individuals based on their contributions to movies by way of "raw SQL" query.
        // The underlying idea is that ordering by the amount of votes returns a set of individuals one might expect to be "popular" on a imdb-like platform
        // The other one is ordering by name rating which is not necessarily the same.
        // Prime example being the actors who have a rating of 10 which actually indicates a very low number of votes and thus not very popular at all.
        //
        // Some extra note is that mapping to a DTO for some reason required the use of escaped double quotes around the column names in order to match the DTO property names.
        //
        // One might have considered to do this as some sort of view or function in the database, 
        // however owing to time constraints and the fact that this is a one-off query, it was simpler to just do it this way.
        var individuals = _ctx.Database
            .SqlQueryRaw<IndividualReferenceWithTotalVotesDTO>(
                    "SELECT i.iconst AS \"Id\", i.name AS \"Name\", p.pconst AS \"PageId\", SUM(t.numvotes) AS \"TotalVotes\" FROM mdb.individual i JOIN mdb.contributor c ON i.iconst = c.iconst JOIN mdb.title t ON c.tconst = t.tconst JOIN mdb.page p ON i.iconst = p.iconst WHERE c.contribution IN ('actor', 'actress') AND t.media_type = 'movie' GROUP BY i.iconst, i.name, p.pconst ORDER BY \"TotalVotes\" DESC LIMIT 100"
                    )
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return individuals;
    }

    // Get titles associated with an individual
    public List<TitlePreviewDTO> TitlesByIndividual(string iconst)
    {
        var titles = _ctx.Titles
            .Where(t => t.Contributors.Any(c => c.Iconst == iconst))
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
