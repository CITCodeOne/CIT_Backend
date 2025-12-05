using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusinessLayer;
using BusinessLayer.DTOs;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/users")]
public class UsersController : ControllerBase
{
    private readonly MdbService _mdbService;

    public UsersController(MdbService mdbService)
    {
        _mdbService = mdbService;
    }

    // GET: api/v2/users/{userId}
    [HttpGet("{userId}")]
    public IActionResult GetUser(int userId)
    {
        var user = _mdbService.User.GetUserById(userId);
        if (user == null) return NotFound(new { message = $"User with id '{userId}' not found" });
        return Ok(user);
    }

    // GET: api/v2/users/{userId}/bookmarks
    [HttpGet("{userId}/bookmarks")]
    public IActionResult GetBookmarks(int userId)
    {
        var bookmarks = _mdbService.Bookmark.GetBookmarksByUser(userId);
        if (bookmarks.Count == 0) return Ok(new List<BookmarkDTO>());
        return Ok(bookmarks);
    }

    // GET: api/v2/users/{userId}/bookmarks/{pageId}
    [HttpGet("{userId}/bookmarks/{pageId}")]
    public IActionResult GetBookmark(int userId, int pageId)
    {
        var bookmark = _mdbService.Bookmark.GetBookmark(userId, pageId);
        if (bookmark == null) return NotFound(new { message = $"Bookmark for user '{userId}' with page '{pageId}' not found" });
        return Ok(bookmark);
    }

    // POST: api/v2/users/{userId}/bookmarks
    [HttpPost("{userId}/bookmarks")]
    [Authorize]
    public IActionResult CreateBookmark(int userId, CreateBookmarkDTO bookmarkCreateDto)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });

        if (authenticatedUserId != userId)
            return Forbid();

        try
        {
            var added = _mdbService.Bookmark.AddBookmark(userId, bookmarkCreateDto.PageId);
            if (added == null)
                return Conflict(new { message = "Bookmark already exists" });

            return Ok(new { message = "Bookmark added", bookmark = added });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE: api/v2/users/{userId}/bookmarks/{pageId}
    [HttpDelete("{userId}/bookmarks/{pageId}")]
    [Authorize]
    public IActionResult DeleteBookmark(int userId, int pageId)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });

        if (authenticatedUserId != userId)
            return Forbid();

        var removed = _mdbService.Bookmark.RemoveBookmark(userId, pageId);
        if (!removed)
            return NotFound(new { message = "Bookmark not found" });

        return Ok(new { message = "Bookmark removed" });
    }

    // GET: api/v2/users/{userId}/ratings
    [HttpGet("{userId}/ratings")]
    public IActionResult GetRatings(int userId)
    {
        var ratings = _mdbService.Rating.GetRatings(userId, null);
        if (ratings.Count == 0)
            return Ok(new List<RatingDTO>());

        return Ok(ratings);
    }

    // POST: api/v2/users/{userId}/ratings
    // PUT: api/v2/users/{userId}/ratings/{titleId}
    // DELETE: api/v2/users/{userId}/ratings/{titleId}
}
