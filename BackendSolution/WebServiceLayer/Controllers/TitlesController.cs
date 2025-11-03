using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using BusinessLayer.DTOs;

namespace WebServiceLayer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TitlesController : ControllerBase
{
    private readonly MdbService _mdbService;

    public TitlesController(MdbService mdbService)
    {
        _mdbService = mdbService;
    }

    // GET: api/titles/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TitleFullDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TitleFullDTO> GetTitle(string id)
    {
        var title = _mdbService.Title.GetTitleById(id);

        if (title == null) return NotFound(new { message = $"Title with id '{id}' not found" });
        return Ok(title);
    }

    // GET: api/titles?page=1&pageSize=20
    [HttpGet]
    [ProducesResponseType(typeof(List<TitlePreviewDTO>), StatusCodes.Status200OK)]
    public ActionResult<List<TitlePreviewDTO>> GetTitles([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var titles = _mdbService.Title.GetTitles(page, pageSize);
        return Ok(titles);
    }

    // GET: api/titles/preview/{id}
    [HttpGet("preview/{id}")]
    [ProducesResponseType(typeof(TitlePreviewDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TitlePreviewDTO> GetTitlePreview(string id)
    {
        var title = _mdbService.Title.GetTitlePreview(id);

        if (title == null) return NotFound(new { message = $"Title with id '{id}' not found" });
        return Ok(title);
    }
}
