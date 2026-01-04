using AutoMapper;
using DataAccessLayer.Data;
using BusinessLayer.DTOs;
using BusinessLayer.Parameters;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

// Service der håndterer søgning og opslag for titler (film/serier).
// Forklaring for ikke-teknikere: denne klasse laver forespørgsler mod databasen
// for at finde titler baseret på brugerens søgekriterier, hente detaljer om en titel,
// og finde relaterede titler.
public class TitleService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    // Konstruktør: får databasekontekst og mapperen injiceret
    public TitleService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx; // Database context
        _mapper = mapper; // Mapper til DTO'er
    }

    // Søg efter titler ud fra flexible parametre (genre, navn, rating, år osv.).
    // Trin for trin: bygges en forespørgsel, filtreres, sorteres og pagineres.
    public List<TitlePreviewDTO> SearchTitles(TitleSearchParameters parameters)
    {
        var query = _ctx.Titles.AsQueryable();

        // Filtrér på minimum rating hvis angivet, ellers behold kun titler med rating
        if (parameters.MinRating.HasValue)
            query = query.Where(t => t.AvgRating >= parameters.MinRating.Value);
        else
            query = query.Where(t => t.AvgRating.HasValue);

        // Filtrér på genre hvis påkrævet. Dette inkluderer også genrelisterne for titlen
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

        // Sortér baseret på brugerens valg (år, titel, rating eller antal stemmer som default)
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
            _ => query.OrderByDescending(t => t.Numvotes)
        };

        var titles = query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .Include(t => t.TitlePage)
            .ToList();

        return _mapper.Map<List<TitlePreviewDTO>>(titles);
    }

    // Hent fuld information om en titel (inkl. genrer og sideinfo)
    public TitleFullDTO? GetTitleById(string tconst)
    {
        var title = _ctx.Titles
            .Include(t => t.Gconsts)  // Load genres for mapping
            .Include(t => t.TitlePage)
            .FirstOrDefault(t => t.Tconst == tconst);

        return title == null ? null : _mapper.Map<TitleFullDTO>(title);
    }

    // Hent en letvægts-udgave af titlen (til lister)
    public TitlePreviewDTO? GetTitlePreview(string tconst)
    {
        var title = _ctx.Titles
            .Include(t => t.TitlePage)
            .FirstOrDefault(t => t.Tconst == tconst);
        return title == null ? null : _mapper.Map<TitlePreviewDTO>(title);
    }

    // Hent "dagens" featured title.
    // Forklaring: vælger en titel ud fra dagen ved at lave en deterministisk udvælgelse
    // af alle kandidater (fx film med høj rating og plot) og bruge dagen som indeks.
    public TitleFullDTO GetFeaturedTitle()
    {
        var featurableCount = _ctx.Titles
            .Where(t => t.AvgRating >= 7.0 && (t.IsAdult.HasValue ? !t.IsAdult.Value : true) && t.Plot != null)
            .Count();

        var daysSinceEpoch = (DateTime.UtcNow - new DateTime(1970, 1, 1)).Days;
        var featureIndex = daysSinceEpoch % featurableCount;

        var featuredTitle = _ctx.Titles
            .Where(t => t.AvgRating >= 7.0 && (t.IsAdult.HasValue ? !t.IsAdult.Value : true) && t.Plot != null)
            .OrderBy(t => t.Tconst)
            .Include(t => t.Gconsts)
            .Skip(featureIndex)
            .Take(1)
            .Include(t => t.TitlePage)
            .FirstOrDefault();

        return _mapper.Map<TitleFullDTO>(featuredTitle);
    }

    // Hent top-titler for en given type (fx "movie" eller "tv")
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

    // Hent personer (skuespillere mv.) associeret med en titel
    public List<IndividualReferenceDTO> GetIndividualsByTitle(string tconst)
    {
        var individuals = _ctx.Individuals
            .Where(i => i.Contributors.Any(c => c.Tconst == tconst))
            .Include(i => i.IndividualPage)
            .ToList();
        return _mapper.Map<List<IndividualReferenceDTO>>(individuals);
    }

    // Få lignende titler sammen med side-id (bruges til at linke til siden)
    public List<SimilarTitleDTO> GetSimilarMoviesWithPageId(string tconst)
    {
        if (string.IsNullOrWhiteSpace(tconst))
            return new List<SimilarTitleDTO>();

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
