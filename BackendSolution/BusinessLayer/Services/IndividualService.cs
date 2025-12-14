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

    // Get co-actors for a given actor name
    public List<CoActorsDTO> GetCoActors(string actorName)
    {
        var coActors = _ctx.Database.SqlQuery<CoActorsDTO>(
            $"SELECT iconst as Iconst, primaryname as Primaryname, co_count as Co_Count FROM mdb.find_co_actors('{actorName}')")
            .ToList();
        return coActors;
    }

    // Search individuals by name and get their contributions
    public List<IndividualSearchResultDTO> SearchIndividuals(string name)
    {
        var results = _ctx.Database.SqlQuery<IndividualSearchResultDTO>(
            $"SELECT iconst as Id, name as Name, contribution as Contribution, title_name as TitleName, detail as Detail, genre as Genre FROM mdb.find_name('{name}')")
            .ToList();
        return results;
    }
}
