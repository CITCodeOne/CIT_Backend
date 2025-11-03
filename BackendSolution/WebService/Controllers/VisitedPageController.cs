using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Entities;
using DataService.DTOs;
using DataService.Data;

namespace WebService.Controllers;

[ApiController]
[Route("api/visited")]
public class VisitedPageController : ControllerBase
{
  private readonly CITContext _context;
  private readonly IMapper _mapper;

  public VisitedPageController(CITContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  // GET: api/visited/user/{userId}
  // Fetches all pages visited by a specific user
  [HttpGet("user/{userId}")]
  public async Task<ActionResult<List<VisitedPageWithDetailsDTO>>> GetUserVisitedPages(int userId)
  {
    var visitedPages = await _context.VisitedPages
      .Include(vp => vp.PconstNavigation)
      .Where(vp => vp.Uconst == userId)
      .OrderByDescending(vp => vp.Time)
      .ToListAsync();

    return Ok(_mapper.Map<List<VisitedPageWithDetailsDTO>>(visitedPages));
  }

  // POST: api/visited
  // Adds a new visited page entry for a user
  [HttpPost]
  public async Task<ActionResult<VisitedPageDTO>> AddVisitedPage(CreateVisitedPageDTO createDto, [FromQuery] int userId)
  {
    // Check if user exists
    var userExists = await _context.UserInfos.AnyAsync(u => u.Uconst == userId);
    if (!userExists)
    {
      return NotFound("User not found");
    }

    // Check if page exists and load it
    var page = await _context.Pages.FirstOrDefaultAsync(p => p.Pconst == createDto.PageId);
    if (page == null)
    {
      return NotFound("Page not found");
    }

    var visitedPage = new VisitedPage
    {
      Uconst = userId,
      Pconst = createDto.PageId,
      Time = DateTime.UtcNow,
      PconstNavigation = page
    };

    _context.VisitedPages.Add(visitedPage);
    await _context.SaveChangesAsync();

    return Ok(_mapper.Map<VisitedPageDTO>(visitedPage));
  }
}
