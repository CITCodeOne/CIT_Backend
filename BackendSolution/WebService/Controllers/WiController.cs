using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Data;
using DataService.DTOs;

namespace WebService.Controllers;

[ApiController]
[Route("api/wi")]
public class WiController : ControllerBase
{
  private readonly CITContext _context;
  private readonly IMapper _mapper;

  public WiController(CITContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  // GET: api/wi/title/{tconst}
  // All word-index rows for a given title
  [HttpGet("title/{tconst}")]
  public async Task<ActionResult<List<WiDTO>>> GetByTitle(string tconst)
  {
    var rows = await _context.Wis
      .Where(w => w.Tconst == tconst)
      .OrderBy(w => w.Field)
      .ThenBy(w => w.Word)
      .ToListAsync();

    return Ok(_mapper.Map<List<WiDTO>>(rows));
  }

  // GET: api/wi/word/{word}
  // All titles that contain a specific word
  [HttpGet("word/{word}")]
  public async Task<ActionResult<List<WiDTO>>> GetByWord(string word)
  {
    var rows = await _context.Wis
      .Where(w => w.Word == word)
      .OrderBy(w => w.Tconst)
      .ThenBy(w => w.Field)
      .ToListAsync();

    return Ok(_mapper.Map<List<WiDTO>>(rows));
  }
}
