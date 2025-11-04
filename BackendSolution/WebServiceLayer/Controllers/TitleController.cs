using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebServiceLayer.Controllers;

[ApiController]
[Route("api/titles")] // Plural resource naming
public class TitleController : ControllerBase
{
    private readonly ITitleService _titles;

    public TitleController(ITitleService titles)
    {
        _titles = titles;
    }

    // GET api/titles/{tconst}
    [HttpGet("{tconst}")]
    public async Task<ActionResult<TitleFullDTO>> GetById(string tconst, CancellationToken ct)
    {
        var dto = await _titles.GetByIdAsync(tconst, ct);
        if (dto is null) return NotFound();
        return Ok(dto);
    }

    // GET api/titles/genre/{genreId}?page=1&pageSize=20
    [HttpGet("genre/{genreId}")]
    public async Task<ActionResult<IReadOnlyList<TitlePreviewDTO>>> GetByGenre(
        int genreId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var list = await _titles.GetByGenreAsync(genreId, page, pageSize, ct);
        // Return 200 with [] when empty (not 404)
        return Ok(list);
    }
}
