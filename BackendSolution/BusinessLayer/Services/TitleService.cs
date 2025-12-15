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
            .Include(t => t.TitlePage)
            .FirstOrDefault(t => t.Tconst == tconst);

        return title == null ? null : _mapper.Map<TitleFullDTO>(title);
    }

    // Get title preview (lightweight)
    public TitlePreviewDTO? GetTitlePreview(string tconst)
    {
        var title = _ctx.Titles
            .Include(t => t.TitlePage)
            .FirstOrDefault(t => t.Tconst == tconst);
        return title == null ? null : _mapper.Map<TitlePreviewDTO>(title);
    }

    // Get multiple titles (for lists)
    // FIX: Change to "keyset paging"
    public List<TitlePreviewDTO> GetTitles(int page = 1, int pageSize = 20)
    {
        var titles = _ctx.Titles
            .Where(t => t.AvgRating.HasValue)
            .OrderByDescending(t => t.AvgRating)
            .Include(t => t.TitlePage)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return _mapper.Map<List<TitlePreviewDTO>>(titles);
    }

    // Get top titles by media type
    public List<TitlePreviewDTO> GetTopTitlesByType(string type, int page = 1, int pageSize = 20)
    {
        var titles = _ctx.Titles
            .Where(t => t.MediaType != null && t.MediaType == type && t.AvgRating.HasValue)
            .OrderByDescending(t => t.AvgRating)
            .Include(t => t.TitlePage)
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

    // Get similar movies based on overlapping genres
    public List<SimilarTitleDTO> GetSimilarMovies(string tconst)
    {
        if (string.IsNullOrWhiteSpace(tconst))
            return new List<SimilarTitleDTO>();

        // TODO: Parameterized call to avoid injection and ensure correct binding
        var similarTitles = _ctx.Database.SqlQuery<SimilarTitleDTO>(
            $"SELECT similar_tconst AS Id, title_name AS Name, COALESCE(overlap_genres, 0) AS OverlapGenres FROM mdb.similar_movies({tconst})")
            .ToList();
        return similarTitles;
    }
}
