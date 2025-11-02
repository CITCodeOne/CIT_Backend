using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Data;
using DataService.DTOs;

namespace WebService.Controllers;

[ApiController]
[Route("api/episode")]
public class EpisodeController : ControllerBase
{
  private readonly CITContext _context;
  private readonly IMapper _mapper;

  public EpisodeController(CITContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  // Get one single episode by id
  [HttpGet("{tconst}")]
  public async Task<ActionResult<EpisodeDTO>> GetOneEpisodeById(string tconst)
  {
    var ep = await _context.Episodes
      .FirstOrDefaultAsync(e => e.Tconst == tconst);

    if (ep == null)
    {
      return NotFound();
    }

    return Ok(_mapper.Map<EpisodeDTO>(ep));
  }

  // Get list for for a series, ordered by season then episode
  [HttpGet("parent/{parenttconst}")]
  public async Task<ActionResult<List<EpisodeDTO>>> GetListForEpisodes(string parenttconst)
  {
    var episodes = await _context.Episodes
      .Where(e => e.Parenttconst == parenttconst)
      .OrderBy(e => e.Snum)
      .ThenBy(e => e.Epnum)
      .ToListAsync();

    return Ok(_mapper.Map<List<EpisodeDTO>>(episodes));
  }
}
