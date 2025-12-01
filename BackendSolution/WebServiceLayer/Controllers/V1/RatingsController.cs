using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusinessLayer;
using BusinessLayer.DTOs;

namespace WebServiceLayer.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
public class RatingsController : ControllerBase
{
    private readonly MdbService _mdb;

    public RatingsController(MdbService mdbService)
    {
        _mdb = mdbService;
    }

    // GET: api/ratings/{uconst}/{tconst} "getting a rating by composite key"
    [HttpGet("{uconst}/{tconst}")]
    [ProducesResponseType(typeof(RatingDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<RatingDTO> GetRatingByPath(int uconst, string tconst)
    {
        var rating = _mdb.Rating.GetRating(uconst, tconst);
        if (rating == null) return NotFound(new { message = $"Rating for user '{uconst}' with title '{tconst}' not found" });
        return Ok(rating);
    }

    // GET: api/ratings?userId={uconst} or api/ratings?titleId={tconst}  "getting ratings by single part of composite key"
    [HttpGet]
    [ProducesResponseType(typeof(List<RatingDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<RatingDTO>> GetRating([FromQuery] int? userId, [FromQuery] string? titleId)
    {
        bool uconstProvided = (userId != null);
        bool tconstProvided = !string.IsNullOrEmpty(titleId);
        // Validate input
        if (uconstProvided && tconstProvided) return BadRequest(new { message = "Provide either 'uconst' or 'tconst', not both" });
        if (!uconstProvided && !tconstProvided) return BadRequest(new { message = "Either 'uconst' or 'tconst' must be provided" });

        // Fetch by uconst
        if (uconstProvided)
        {
            var ratingsByUser = _mdb.Rating.GetRatings(userId, null);
            if (ratingsByUser.Count == 0)
            {
                return NotFound(new { message = $"No ratings found for user with id '{userId}'" });
            }
            return Ok(ratingsByUser);
        }
        // Fetch by tconst
        else
        {
            var ratingsByTitle = _mdb.Rating.GetRatings(null, titleId);
            if (ratingsByTitle.Count == 0)
            {
                return NotFound(new { message = $"No ratings found for title with id '{titleId}'" });
            }
            return Ok(ratingsByTitle);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RateTitle(CreateRatingDTO model)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var uconst))
        {
            return Unauthorized(new { message = "User id missing from token" });
        }

        try
        {
            await _mdb.Rating.RateAsync(uconst, model.TitleId, model.Rating);
            return Ok(new { message = "Rating saved" });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            // The database function already enforces constraints; expose a generic error message here
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to save rating", detail = ex.Message });
        }
    }

    [HttpDelete("{titleId}")]
    [Authorize]
    public async Task<IActionResult> DeleteRating(string titleId)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var uconst))
        {
            return Unauthorized(new { message = "User id missing from token" });
        }

        try
        {
            await _mdb.Rating.DeleteRatingAsync(uconst, titleId);
            return Ok(new { message = "Rating removed" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to delete rating", detail = ex.Message });
        }
    }

}
