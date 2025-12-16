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
    // We considered keyset paging but kept offset paging for simplicity
    // Really taking advantage of keyset paging would require more significant changes to multiple things
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

    // Get titles by genre
    public List<TitlePreviewDTO> GetTitles(int page, int pageSize, string genre)
    {
        var titles = _ctx.Titles
            .Include(t => t.Gconsts)
            .Where(t => t.AvgRating.HasValue && t.Gconsts.Any(g => g.Gname == genre))
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

        // Parameterized call to avoid injection and ensure correct binding
        // SqlQueryRaw instead of SqlQuery allows for parameterization in EF Core
        var similarTitles = _ctx.Database
            .SqlQueryRaw<SimilarTitleDTO>("SELECT similar_tconst AS Id, title_name AS Name, COALESCE(overlap_genres, 0) AS OverlapGenres FROM mdb.similar_movies({0})", tconst)
            .ToList();
        return similarTitles;
    }

    // Get todays featured title

    public TitleFullDTO GetFeaturedTitle()
    {
        // Gathers a list of "featurable" titles (i.e. rating above a threshold and non-adult with a plot)
        // the length of this list is used as a modulus to pick a title based on the current day
        // the current day is found as a nuber by taking the number of days since Unix epoch
        var featurableCount = _ctx.Titles
            .Where(t => t.AvgRating >= 7.0 && (t.IsAdult.HasValue ? !t.IsAdult.Value : true) && t.Plot != null)
            .Count();

        var daysSinceEpoch = (DateTime.UtcNow - new DateTime(1970, 1, 1)).Days;
        var featureIndex = daysSinceEpoch % featurableCount;

        var featuredTitle = _ctx.Titles
            .Where(t => t.AvgRating >= 7.0 && (t.IsAdult.HasValue ? !t.IsAdult.Value : true) && t.Plot != null)
            .OrderBy(t => t.Tconst) // Ensure consistent ordering
            .Include(t => t.Gconsts)
            .Skip(featureIndex)
            .Take(1)
            .FirstOrDefault();

        return _mapper.Map<TitleFullDTO>(featuredTitle);
    }
}
