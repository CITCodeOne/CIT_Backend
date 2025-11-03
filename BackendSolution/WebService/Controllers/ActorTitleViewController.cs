using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Data;
using DataService.DTOs;

namespace WebService.Controllers;

[ApiController]
[Route("api/actortitleview")]
public class ActorTitleViewController : ControllerBase
{
  private readonly CITContext _context;
  private readonly IMapper _mapper;

  public ActorTitleViewController(CITContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  // Get ActorTitleView by Title
  [HttpGet("title/{tconst}")]
  public async Task<ActionResult<List<ActorTitleViewDTO>>> GetByTitle(string tconst)
  {
    var rows = await _context.ActorTitleViews
      .Where(a => a.Tconst == tconst)
      .ToListAsync();

    return Ok(_mapper.Map<List<ActorTitleViewDTO>>(rows));
  }

  // Get ActorTitleView by Individual
  [HttpGet("individual/{iconst}")]
  public async Task<ActionResult<List<ActorTitleViewDTO>>> GetByIndividual(string iconst)
  {
    var rows = await _context.ActorTitleViews
      .Where(a => a.Iconst == iconst)
      .ToListAsync();

    return Ok(_mapper.Map<List<ActorTitleViewDTO>>(rows));
  }
}
