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
}
