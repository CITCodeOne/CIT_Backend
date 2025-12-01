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
    public IActionResult GetUser(int userId)
    {
        var user = _mdbService.User.GetUserById(userId);
        if (user == null)
            return NotFound(new { message = $"User with id '{userId}' not found" });

        return Ok(user);
    }

    // GET: api/v2/users/{userId}/bookmarks
    [HttpGet("{userId}/bookmarks")]
    public IActionResult GetBookmarks(int userId)
    {
        var bookmarks = _mdbService.Bookmark.GetBookmarksByUser(userId);
        if (bookmarks.Count == 0)
            return Ok(new List<BookmarkDTO>());

        return Ok(bookmarks);
    }

    // POST: api/v2/users/{userId}/bookmarks
    // DELETE: api/v2/users/{userId}/bookmarks/{pageId}

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
