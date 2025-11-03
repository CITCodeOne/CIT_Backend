using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Entities;
using DataService.DTOs;
using DataService.Data;

namespace WebService.Controllers;
[ApiController]
[Route("api/title")]
public class TitleController : ControllerBase
{
  private readonly CITContext _context;
  private readonly IMapper _mapper;

  public TitleController(CITContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  [HttpGet("{tconst}")]
  public async Task<ActionResult<TitleFullDTO>> GetTitle(string tconst)
  {
    var title = await _context.Titles
      .Include(t => t.Gconsts) // Include genres
      .FirstOrDefaultAsync(t => t.Tconst == tconst);

    if (title == null)
    {
      return NotFound();
    }

    return Ok(_mapper.Map<TitleFullDTO>(title));
  }

  [HttpGet("genre/{gconst}")]
  public async Task<ActionResult<List<TitlePreviewDTO>>> GetTitlesByGenre(int gconst)
  {
    var title = await _context.Titles
      .Include(t => t.Gconsts) 
      .Where(t => t.Gconsts.Any(g => g.Gconst == gconst))
      .ToListAsync();

    if (title == null)
    {
      return NotFound();
    }

    return Ok(_mapper.Map<List<TitlePreviewDTO>>(title));
  }


}