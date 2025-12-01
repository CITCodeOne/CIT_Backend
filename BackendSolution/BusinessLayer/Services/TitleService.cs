using AutoMapper;
using DataAccessLayer.Data;
using BusinessLayer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

public class TitleService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    // Constructor with dependency injection
    public TitleService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx; // Database context
        _mapper = mapper; // Mapping profile for DTOs
    }

    // Get single title with full details
    public TitleFullDTO? GetTitleById(string tconst)
    {
        var title = _ctx.Titles
            .Include(t => t.Gconsts)  // Load genres for mapping
            .FirstOrDefault(t => t.Tconst == tconst);

        return title == null ? null : _mapper.Map<TitleFullDTO>(title);
    }

    // Get title preview (lightweight)
    public TitlePreviewDTO? GetTitlePreview(string tconst)
    {
        var title = _ctx.Titles.Find(tconst);
        return title == null ? null : _mapper.Map<TitlePreviewDTO>(title);
    }

    // Get multiple titles (for lists)
    // FIX: Change to "keyset paging"
    public List<TitlePreviewDTO> GetTitles(int page = 1, int pageSize = 20)
    {
        var titles = _ctx.Titles
            .OrderByDescending(t => t.AvgRating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return _mapper.Map<List<TitlePreviewDTO>>(titles);
    }

    // Get individuals associated with a title
    public List<IndividualReferenceDTO> GetIndividualsByTitle(string tconst)
    {
        var individuals = _ctx.Individuals
            .Where(i => i.Contributors.Any(c => c.Tconst == tconst))
            .ToList();
        return _mapper.Map<List<IndividualReferenceDTO>>(individuals);
    }
}
