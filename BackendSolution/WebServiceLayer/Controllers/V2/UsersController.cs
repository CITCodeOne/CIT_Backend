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

    // BOOKMARKS

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

    // RATINGS

    // GET: api/v2/users/{userId}/ratings
    [HttpGet("{userId}/ratings")]
    public IActionResult GetRatings(int userId)
    {
        var ratings = _mdbService.Rating.GetRatings(userId, null);
        if (ratings.Count == 0)
            return Ok(new List<RatingDTO>());

        return Ok(ratings);
    }

    // GET: api/v2/users/{userId}/ratings/{titleId}
    [HttpGet("{userId}/ratings/{titleId}")]
    public IActionResult GetRating(int userId, string titleId)
    {
        var rating = _mdbService.Rating.GetRating(userId, titleId);
        if (rating == null)
            return NotFound(new { message = $"Rating for user '{userId}' with title '{titleId}' not found" });

        return Ok(rating);
    }

    // POST: api/v2/users/{userId}/ratings
    [HttpPost("{userId}/ratings")]
    [Authorize]
    public IActionResult CreateRating(int userId, CreateRatingDTO ratingCreateDto)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });

        if (authenticatedUserId != userId)
            return Forbid();

        try
        {
            _mdbService.Rating.RateAsync(userId, ratingCreateDto.TitleId, ratingCreateDto.Rating).Wait();
            return Ok(new { message = "Rating submitted" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT: api/v2/users/{userId}/ratings/{titleId}
    [HttpPut("{userId}/ratings/{titleId}")]
    [Authorize]
    public IActionResult UpdateRating(int userId, string titleId, UpdateRatingDTO ratingUpdateDto)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });

        if (authenticatedUserId != userId) return Forbid();

        // ensure rating exists before updating
        var existingRating = _mdbService.Rating.GetRating(userId, titleId);
        if (existingRating == null)
            return NotFound(new { message = $"Rating for user '{userId}' with title '{titleId}' not found" });

        try
        {
            _mdbService.Rating.RateAsync(userId, titleId, ratingUpdateDto.Rating).Wait();
            return Ok(new { message = "Rating updated" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE: api/v2/users/{userId}/ratings/{titleId}
    [HttpDelete("{userId}/ratings/{titleId}")]
    [Authorize]
    public IActionResult DeleteRating(int userId, string titleId)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });

        if (authenticatedUserId != userId) return Forbid();

        // ensure rating exists before deleting
        var existingRating = _mdbService.Rating.GetRating(userId, titleId);
        if (existingRating == null)
            return NotFound(new { message = $"Rating for user '{userId}' with title '{titleId}' not found" });

        try
        {
            _mdbService.Rating.DeleteRatingAsync(userId, titleId).Wait();
            return Ok(new { message = "Rating deleted" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PROFILE IMAGE
    // GET: api/v2/users/{userId}/profile-image
    [HttpGet("{userId}/profile-image")]
    public IActionResult GetProfileImage(int userId)
    {
        var image = _mdbService.User.GetProfileImage(userId);
        if (image == null) return NotFound(new { message = $"User '{userId}' has no profile image" });
        return Ok(new UserProfileImageDTO { UserId = userId, ProfileImage = image });
    }

    // PUT: api/v2/users/{userId}/profile-image
    [HttpPut("{userId}/profile-image")]
    [Authorize]
    public IActionResult UpsertProfileImage(int userId, UpdateProfileImageDTO dto)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });
        if (authenticatedUserId != userId) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.ImageBase64))
            return BadRequest(new { message = "ImageBase64 cannot be empty" });

        var saved = _mdbService.User.SetProfileImage(userId, dto.ImageBase64);
        if (!saved) return NotFound(new { message = $"User '{userId}' not found" });

        return Ok(new { message = "Profile image saved" });
    }
    //skal lave post fordi put er til opdatering og ikke oprettelse ifølge REST

}
