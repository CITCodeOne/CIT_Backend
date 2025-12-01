using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusinessLayer;
using BusinessLayer.DTOs;
using WebServiceLayer.Models;

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
    [Authorize]
    public IActionResult GetUser(int userId)
    {
        var user = _mdbService.User.GetUserById(userId);
        if (user == null)
            return NotFound(new { message = $"User with id '{userId}' not found" });

        return Ok(user);
    }

    // GET: api/v2/users/{userId}/bookmarks
    [HttpGet("{userId}/bookmarks")]
    [Authorize]
    public IActionResult GetBookmarks(int userId)
    {
        // Verify user can only access their own bookmarks
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var currentUserId))
            return Unauthorized();

        if (userId != currentUserId)
            return Forbid();

        var bookmarks = _mdbService.Bookmark.GetBookmarksByUser(userId);
        if (bookmarks.Count == 0)
            return Ok(new List<BookmarkDTO>());

        return Ok(bookmarks);
    }

    // POST: api/v2/users/{userId}/bookmarks
    [HttpPost("{userId}/bookmarks")]
    [Authorize]
    public IActionResult AddBookmark(int userId, CreateBookmarkModel model)
    {
        // Verify user can only add to their own bookmarks
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var currentUserId))
            return Unauthorized();

        if (userId != currentUserId)
            return Forbid();

        try
        {
            var added = _mdbService.Bookmark.AddBookmark(userId, model.Pconst);
            if (!added)
                return Conflict(new { message = "Bookmark already exists" });

            return Created($"/api/v2/users/{userId}/bookmarks/{model.Pconst}", new { message = "Bookmark added" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE: api/v2/users/{userId}/bookmarks/{bookmarkId}
    [HttpDelete("{userId}/bookmarks/{bookmarkId}")]
    [Authorize]
    public IActionResult RemoveBookmark(int userId, int bookmarkId)
    {
        // Verify user can only delete their own bookmarks
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var currentUserId))
            return Unauthorized();

        if (userId != currentUserId)
            return Forbid();

        var removed = _mdbService.Bookmark.RemoveBookmark(userId, bookmarkId);
        if (!removed)
            return NotFound(new { message = "Bookmark not found" });

        return NoContent();
    }

    // GET: api/v2/users/{userId}/ratings
    [HttpGet("{userId}/ratings")]
    [Authorize]
    public IActionResult GetRatings(int userId)
    {
        // Verify user can only access their own ratings
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var currentUserId))
            return Unauthorized();

        if (userId != currentUserId)
            return Forbid();

        var ratings = _mdbService.Rating.GetRatings(userId, null);
        if (ratings.Count == 0)
            return Ok(new List<RatingDTO>());

        return Ok(ratings);
    }

    // POST: api/v2/users/{userId}/ratings
    [HttpPost("{userId}/ratings")]
    [Authorize]
    public IActionResult CreateRating(int userId, [FromBody] CreateRatingModel model)
    {
        // Verify user can only create their own ratings
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var currentUserId))
            return Unauthorized();

        if (userId != currentUserId)
            return Forbid();

        try
        {
            var created = _mdbService.Rating.CreateRating(userId, model.TitleId, model.Rating);
            if (!created)
                return Conflict(new { message = "Rating already exists for this title" });

            return Created($"/api/v2/users/{userId}/ratings/{model.TitleId}", new { message = "Rating created" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT: api/v2/users/{userId}/ratings/{titleId}
    [HttpPut("{userId}/ratings/{titleId}")]
    [Authorize]
    public IActionResult UpdateRating(int userId, string titleId, [FromBody] UpdateRatingModel model)
    {
        // Verify user can only update their own ratings
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var currentUserId))
            return Unauthorized();

        if (userId != currentUserId)
            return Forbid();

        try
        {
            var updated = _mdbService.Rating.UpdateRating(userId, titleId, model.Rating);
            if (!updated)
                return NotFound(new { message = "Rating not found" });

            return Ok(new { message = "Rating updated" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE: api/v2/users/{userId}/ratings/{titleId}
    [HttpDelete("{userId}/ratings/{titleId}")]
    [Authorize]
    public IActionResult DeleteRating(int userId, string titleId)
    {
        // Verify user can only delete their own ratings
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var currentUserId))
            return Unauthorized();

        if (userId != currentUserId)
            return Forbid();

        var deleted = _mdbService.Rating.DeleteRating(userId, titleId);
        if (!deleted)
            return NotFound(new { message = "Rating not found" });

        return NoContent();
    }
}
