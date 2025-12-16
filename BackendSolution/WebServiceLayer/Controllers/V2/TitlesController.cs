using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using BusinessLayer.DTOs;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/titles")]
public class TitlesController : ControllerBase
{
    private readonly MdbService _mdbService;

    public TitlesController(MdbService mdbService)
    {
        _mdbService = mdbService;
    }

    // GET: api/v2/titles?page=1&pageSize=20
    [HttpGet]
    [ProducesResponseType(typeof(List<TitlePreviewDTO>), StatusCodes.Status200OK)]
    public ActionResult<List<TitlePreviewDTO>> GetTitles([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? genre = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        if (!string.IsNullOrWhiteSpace(genre))
        {
            var titlesByGenre = _mdbService.Title.GetTitles(page, pageSize, genre);
            return Ok(titlesByGenre);
        }

        var titles = _mdbService.Title.GetTitles(page, pageSize);
        return Ok(titles);
    }

    // GET: api/v2/titles/top/{type}?page=1&pageSize=20
    [HttpGet("top/{type}")]
    [ProducesResponseType(typeof(List<TitlePreviewDTO>), StatusCodes.Status200OK)]
    public ActionResult<List<TitlePreviewDTO>> GetTopTitles(string type, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
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
        var titles = _mdbService.Title.GetFeaturedTitle();
        return Ok(titles);
    }

    // GET: api/v2/titles/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TitleFullDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TitleFullDTO> GetTitle(string id)
    {
        var title = _mdbService.Title.GetTitleById(id);

        if (title == null)
            return NotFound(new { message = $"Title with id '{id}' not found" });

        return Ok(title);
    }

    // GET: api/v2/titles/{id}/ratings
    [HttpGet("{id}/ratings")]
    [ProducesResponseType(typeof(List<RatingDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<RatingDTO>> GetTitleRatings(string id)
    {
        // Verify title exists
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
        // Verify title exists
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
        // Verify title exists
        var title = _mdbService.Title.GetTitleById(id);
        if (title == null)
            return NotFound(new { message = $"Title with id '{id}' not found" });

        var similarMovies = _mdbService.Title.GetSimilarMovies(id);
        return Ok(similarMovies);
    }
}

