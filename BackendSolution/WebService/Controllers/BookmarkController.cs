using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Entities;
using DataService.DTOs;
using DataService.Data;

namespace WebService.Controllers;

[ApiController]
[Route("api/bookmark")]
public class BookmarkController : ControllerBase
{
  private readonly CITContext _context;
  private readonly IMapper _mapper;

  public BookmarkController(CITContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  // GET: api/bookmark/user/{userId}
  // Fetches all bookmarks for a specific user
  [HttpGet("user/{userId}")]
  public async Task<ActionResult<List<BookmarkWithDetailsDTO>>> GetUserBookmarks(int userId)
  {
    var bookmarks = await _context.Bookmarks
      .Include(b => b.PconstNavigation)
      .Where(b => b.Uconst == userId)
      .OrderByDescending(b => b.Time)
      .ToListAsync();

    return Ok(_mapper.Map<List<BookmarkWithDetailsDTO>>(bookmarks));
  }

  // GET: api/bookmark/check?userId={userId}&pageId={pageId}
  // Check if a user has bookmarked a specific page
  [HttpGet("check")]
  public async Task<ActionResult<object>> CheckBookmark([FromQuery] int userId, [FromQuery] int pageId)
  {
    var bookmark = await _context.Bookmarks
      .FirstOrDefaultAsync(b => b.Uconst == userId && b.Pconst == pageId);

    return Ok(new { isBookmarked = bookmark != null });
  }

  // POST: api/bookmark
  // Adds a new bookmark for a user
  [HttpPost]
  public async Task<ActionResult<BookmarkDTO>> AddBookmark(CreateBookmarkDTO createDto, [FromQuery] int userId)
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

    // Check if bookmark already exists
    var existingBookmark = await _context.Bookmarks
      .FirstOrDefaultAsync(b => b.Uconst == userId && b.Pconst == createDto.PageId);

    if (existingBookmark != null)
    {
      return Conflict("Bookmark already exists");
    }

    var bookmark = new Bookmark
    {
      Uconst = userId,
      Pconst = createDto.PageId,
      Time = DateTime.UtcNow,
      PconstNavigation = page
    };

    _context.Bookmarks.Add(bookmark);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(CheckBookmark), new { userId, pageId = createDto.PageId }, _mapper.Map<BookmarkDTO>(bookmark));
  }

  // DELETE: api/bookmark/{userId}/{pageId}
  // Deletes a bookmark
  [HttpDelete("{userId}/{pageId}")]
  public async Task<ActionResult> DeleteBookmark(int userId, int pageId)
  {
    var bookmark = await _context.Bookmarks
      .FirstOrDefaultAsync(b => b.Uconst == userId && b.Pconst == pageId);

    if (bookmark == null)
    {
      return NotFound("Bookmark not found");
    }

    _context.Bookmarks.Remove(bookmark);
    await _context.SaveChangesAsync();

    return NoContent();
  }
}