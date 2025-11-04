using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusinessLayer;
using BusinessLayer.Services;
using WebServiceLayer.Models;
using System.Security.Claims;

namespace WebServiceLayer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookmarksController : ControllerBase
{
    private readonly MdbService _mdbService;

    public BookmarksController(MdbService mdbService)
    {
        _mdbService = mdbService;
    }

    [HttpPost]
    [Authorize]
    public IActionResult AddBookmark(CreateBookmarkModel model)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var uconst))
            return Unauthorized();

        try
        {
            var added = _mdbService.Bookmark.AddBookmark(uconst, model.Pconst);
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
