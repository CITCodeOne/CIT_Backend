using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusinessLayer.DTOs;
using BusinessLayer;
// using System.Security.Claims; // Was needed at some point, but now unused apparently

namespace WebServiceLayer.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
public class BookmarksController : ControllerBase
{
    private readonly MdbService _mdbService;

    public BookmarksController(MdbService mdbService)
    {
        _mdbService = mdbService;
    }

    // GET: api/bookmarks
    [HttpGet]
    [Authorize]
    public IActionResult GetBookmarks()
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var uconst))
            return Unauthorized();

        var bookmarks = _mdbService.Bookmark.GetBookmarksByUser(uconst);
        if (bookmarks.Count == 0) return NotFound(new { message = "No bookmarks found for the user" });
        return Ok(bookmarks);
    }

    // DELETE: api/bookmarks/{pconst}
    [HttpDelete("{pconst}")]
    [Authorize]
    public IActionResult RemoveBookmark(int pconst)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var uconst))
            return Unauthorized();

        var removed = _mdbService.Bookmark.RemoveBookmark(uconst, pconst);
        if (!removed)
            return NotFound(new { message = "Bookmark not found" });
        return Ok(new { message = "Bookmark removed" });
    }

    // POST: api/bookmarks
    [HttpPost]
    [Authorize]
    public IActionResult AddBookmark(CreateBookmarkDTO model)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var uconst))
            return Unauthorized();

        try
        {
            var added = _mdbService.Bookmark.AddBookmark(uconst, model.PageId);
            if (!added)
                return Conflict(new { message = "Bookmark already exists" });

            return Ok(new { message = "Bookmark added" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
