using Microsoft.AspNetCore.Mvc;
using DataService.Entities;
using DataService.DTOs;
using DataService.Data;

namespace WebService.Controllers;
[ApiController]
[Route("api/title")]
public class TitleController : ControllerBase
{
  private readonly CITContext _context;

  public TitleController(CITContext context)
  {
    _context = context;
  }

  [HttpGet("{tconst}")]
  public async Task<ActionResult<TitleFullDTO>> GetTitle(string tconst)
  {
    var title = await _context.Titles.FindAsync(tconst);

    if (title == null)
    {
      return NotFound();
    }

    return Ok(title);
  }
}