using AutoMapper;
using DataAccessLayer.Data;
using BusinessLayer.DTOs;
using BusinessLayer.Parameters;
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

    // Search titles with flexible parameters
    public List<TitlePreviewDTO> SearchTitles(TitleSearchParameters parameters)
    {
        // Gets titles as queryable for building
        var query = _ctx.Titles.AsQueryable();

        // Apply filters conditionally
        if (parameters.MinRating.HasValue)
            query = query.Where(t => t.AvgRating >= parameters.MinRating.Value);
        else
            query = query.Where(t => t.AvgRating.HasValue); // Default filter

        if (!string.IsNullOrWhiteSpace(parameters.Genre))
        {
            query = query.Include(t => t.Gconsts)
                .Where(t => t.Gconsts
                .Any(g => g.Gname == parameters.Genre));
        }

        if (!string.IsNullOrWhiteSpace(parameters.MediaType))
            query = query.Where(t => t.MediaType == parameters.MediaType);

        if (!string.IsNullOrWhiteSpace(parameters.Name))
            query = query.Where(t => t.TitleName != null && EF.Functions.ILike(t.TitleName, $"%{parameters.Name}%"));

        if (parameters.MinYear.HasValue)
            query = query.Where(t => t.StartYear >= parameters.MinYear.Value);

        if (parameters.MaxYear.HasValue)
            query = query.Where(t => t.StartYear <= parameters.MaxYear.Value);

        if (parameters.IsAdult.HasValue)
            query = query.Where(t => t.IsAdult == parameters.IsAdult.Value);

        // Apply sorting
        query = parameters.SortBy?.ToLower() switch
        {
            "year" => parameters.SortDescending
                ? query.OrderByDescending(t => t.StartYear)
                : query.OrderBy(t => t.StartYear),
            "title" => parameters.SortDescending
                ? query.OrderByDescending(t => t.TitleName)
                : query.OrderBy(t => t.TitleName),
            "rating" => parameters.SortDescending
                ? query.OrderByDescending(t => t.AvgRating)
                : query.OrderBy(t => t.AvgRating),
            _ => query.OrderByDescending(t => t.Numvotes) // Default: numvotes
        };

        // Apply pagination
        var titles = query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Include(t => t.TitlePage)
            .ToList();

        return _mapper.Map<List<TitlePreviewDTO>>(titles);
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

    // Get todays featured title
    public TitleFullDTO GetFeaturedTitle()
    {
        // Exam condition logic for daily featured title to default to a "good" title on the 8th of January 2026
        if (DateTime.UtcNow.Date == new DateTime(2026, 1, 8)) // Exam date
        {
            var featuredTitleExam = _ctx.Titles
                .Where(t => t.Tconst == "tt2084970") // Chosen title
                .Include(t => t.Gconsts)
                .Include(t => t.TitlePage)
                .FirstOrDefault();

            return _mapper.Map<TitleFullDTO>(featuredTitleExam);
        }

        // Logic:
        // 1) Gathers list of "featurable" titles (i.e. rating above a threshold and non-adult with a plot)
        // 2) the length of this list is used as a modulus to pick a title based on the current day
        // 3) the current day is found as a nuber by taking the number of days since Unix epoch
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
            .Include(t => t.TitlePage)
            .FirstOrDefault();

        return _mapper.Map<TitleFullDTO>(featuredTitle);
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
            .Include(i => i.IndividualPage)
            .ToList();
        return _mapper.Map<List<IndividualReferenceDTO>>(individuals);
    }

    // NOTE: slight extension to the old GetSimilarMovies function to get pageId
    // The old one ran on the simpler: SELECT similar_tconst AS Id, title_name AS Name, COALESCE(overlap_genres, 0) AS OverlapGenres FROM mdb.similar_movies({0})
    public List<SimilarTitleDTO> GetSimilarMoviesWithPageId(string tconst)
    {
        if (string.IsNullOrWhiteSpace(tconst))
            return new List<SimilarTitleDTO>();

        // Just makes use of two CTEs to join similar movies with page table to get pageId as well
        // Parameterized call to avoid injection and ensure correct binding
        // SqlQueryRaw instead of SqlQuery allows for parameterization in EF Core
        // Also includes AsNoTracking since this is a read-only query which improves performance slightly (its read only after all)
        var similarTitles = _ctx.Database
            .SqlQueryRaw<SimilarTitleDTO>(
                    @"WITH similar_movies AS (
                        SELECT similar_tconst AS Id, title_name AS Name, COALESCE(overlap_genres, 0) AS OverlapGenres FROM mdb.similar_movies({0})
                    ), page_ids AS (
	                    SELECT tconst AS Id, pconst AS PageId FROM mdb.page
                    ) SELECT * FROM similar_movies NATURAL INNER JOIN page_ids", tconst)
            .AsNoTracking()
            .ToList();
        return similarTitles;
    }
}
