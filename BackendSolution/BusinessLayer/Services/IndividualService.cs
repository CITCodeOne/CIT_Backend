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
        var individual = _ctx.Individuals.Find(iconst);
        return individual == null ? null : _mapper.Map<IndividualReferenceDTO>(individual);
    }

    // Get multiple individuals (for lists)
    // FIX: Change to "keyset paging"
    public List<IndividualReferenceDTO> ReferenceByPage(int page = 1, int pageSize = 20)
    {
        var individuals = _ctx.Individuals
            .OrderByDescending(t => t.NameRating) // WARN: Might cause an issue if nulls are treated as lowest (We have a lot of those in the db)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return _mapper.Map<List<IndividualReferenceDTO>>(individuals);
    }

    // Get most popular individuals, excluding null ratings, paginated
    public List<IndividualReferenceDTO> GetMostPopularIndividuals(int page = 1, int pageSize = 20)
    {
        var individuals = _ctx.Individuals
            .Where(i => i.NameRating != null)
            .OrderByDescending(i => i.NameRating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return _mapper.Map<List<IndividualReferenceDTO>>(individuals);
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
        if (string.IsNullOrWhiteSpace(actorName))
            return new List<CoActorDTO>();

        // Use parameterized raw SQL to avoid injection while letting PostgreSQL execute the function
        var results = _ctx.Database.SqlQuery<CoActorDTO>(
            $"SELECT iconst AS Id, primaryname AS Name, co_count AS CollaborationCount FROM mdb.find_co_actors({actorName})")
            .ToList();

        return results;
    }

    // Search individuals by name and get their contributions (Not sure this should be here)
    public List<IndividualSearchResultDTO> SearchIndividuals(string name)
    {
        var results = _ctx.Database.SqlQuery<IndividualSearchResultDTO>(
            $"SELECT iconst AS Id, name AS Name, contribution AS Contribution, title_name AS TitleName, COALESCE(detail, '') AS Detail, COALESCE(genre, '') AS Genre FROM mdb.find_name({name})")
            .ToList();
        return results;
    }
}
