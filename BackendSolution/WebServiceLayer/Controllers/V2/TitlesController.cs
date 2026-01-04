using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using BusinessLayer.DTOs;
using BusinessLayer.Parameters;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/titles")]
public class TitlesController : ControllerBase
{
    // Controller for titler (film, serier osv.). Den giver klienten
    // mulighed for at søge, hente top-lister, få detaljer og relaterede
    // informationer om en titel. Kommentarerne forklarer hvad hvert
    // endpoint returnerer i almindelige vendinger.
    private readonly MdbService _mdbService;

    public TitlesController(MdbService mdbService)
    {
        // `MdbService` bruges til at hente titel-relaterede data fra
        // forretningslaget/databasen. Controlleren holder kun på
        // hvordan svar formateres og hvilke parametre der accepteres.
        _mdbService = mdbService;
    }

    // Search titles with various parameters
    // INFO: See optional params in the TitleSearchParameters class
    //
    // GET: api/v2/titles?params
    [HttpGet]
    [ProducesResponseType(typeof(List<TitlePreviewDTO>), StatusCodes.Status200OK)]
    public ActionResult<List<TitlePreviewDTO>> SearchTitles([FromQuery] TitleSearchParameters parameters)
    {
        // Søger titler efter de parametre som klienten sender. Parametrene
        // kan indeholde tekstsøgning, filtre og paginering. Vi sikrer at
        // side-nummer og størrelse er i fornuftige intervaller, henter
        // resultaterne fra forretningslaget og returnerer dem.
        if (parameters.Page < 1) parameters.Page = 1;
        if (parameters.PageSize < 1 || parameters.PageSize > 100) parameters.PageSize = 20;

        var titles = _mdbService.Title.SearchTitles(parameters);
        return Ok(titles);
    }

    // GET: api/v2/titles/top/{type}?page=1&pageSize=20
    [HttpGet("top/{type}")]
    [ProducesResponseType(typeof(List<TitlePreviewDTO>), StatusCodes.Status200OK)]
    public ActionResult<List<TitlePreviewDTO>> setTopTitles(string type, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Hent top-titler af en bestemt type (fx "movie" eller "tv").
        // Resultatet er pagineret så klienten kan bede om flere sider.
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var titles = _mdbService.Title.GetTopTitlesByType(type, page, pageSize);
        return Ok(titles);
    }

    // GET: api/v2/titles/featured
    [HttpGet("featured")]
    [ProducesResponseType(typeof(TitleFullDTO), StatusCodes.Status200OK)]
    public ActionResult<List<TitleFullDTO>> GetFeaturedTitles()
    {
        // Returnerer de fremhævede titler som app'en ønsker at vise på
        // forsiden eller lignende. Dette er ofte et håndplukket sæt.
        var titles = _mdbService.Title.GetFeaturedTitle();
        return Ok(titles);
    }

    // GET: api/v2/titles/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TitleFullDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TitleFullDTO> GetTitle(string id)
    {
        // Hent fulde detaljer for en titel (beskrivelse, år, osv.). Hvis
        // titlen ikke findes, svarer vi med NotFound.
        var title = _mdbService.Title.GetTitleById(id);

        if (title == null)
            return NotFound(new { message = $"Title with id '{id}' not found" });

        return Ok(title);
    }

    // GET: api/v2/titles/{id}/page
    [HttpGet("{id}/page")]
    [ProducesResponseType(typeof(PageReferenceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<PageReferenceDTO> GetTitlePages(string id)
    {
        // Returnér sider (artikler/undersider) som hører til en titel. Først
        // tjekker vi at titlen eksisterer, så vi ikke søger efter sider for
        // noget der ikke findes.
        var title = _mdbService.Title.GetTitleById(id);
        if (title == null)
            return NotFound(new { message = $"Title with id '{id}' not found" });
        var pages = _mdbService.Page.GetPageByTitleId(id);
        return Ok(pages);
    }

    // GET: api/v2/titles/{id}/ratings
    [HttpGet("{id}/ratings")]
    [ProducesResponseType(typeof(List<RatingDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<RatingDTO>> GetTitleRatings(string id)
    {
        // Hent alle brugervurderinger (ratings) for en titel. Vi tjekker
        // at titlen findes før vi henter vurderinger.
        var title = _mdbService.Title.GetTitleById(id);
        if (title == null)
            return NotFound(new { message = $"Title with id '{id}' not found" });

        var ratings = _mdbService.Rating.GetRatings(null, id);
        return Ok(ratings);
    }

    // GET: api/v2/titles/{id}/individuals
    [HttpGet("{id}/individuals")]
    [ProducesResponseType(typeof(List<IndividualReferenceDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<IndividualReferenceDTO>> GetTitleIndividuals(string id)
    {
        // Hent personer (skuespillere, instruktører mv.) som er knyttet
        // til titlen. Vi tjekker først at titlen findes.
        var title = _mdbService.Title.GetTitleById(id);
        if (title == null)
            return NotFound(new { message = $"Title with id '{id}' not found" });
        var individuals = _mdbService.Title.GetIndividualsByTitle(id);
        return Ok(individuals);
    }

    // GET: api/v2/titles/{id}/similar
    [HttpGet("{id}/similar")]
    [ProducesResponseType(typeof(List<SimilarTitleDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<SimilarTitleDTO>> GetSimilarMovies(string id)
    {
        // Hent lignende titler (anbefalinger). Vi tjekker at titlen findes
        // og returnerer derefter en liste over titler som minder om den
        // valgte.
        var title = _mdbService.Title.GetTitleById(id);
        if (title == null)
            return NotFound(new { message = $"Title with id '{id}' not found" });

        var similarMovies = _mdbService.Title.GetSimilarMoviesWithPageId(id);
        return Ok(similarMovies);
    }
}

