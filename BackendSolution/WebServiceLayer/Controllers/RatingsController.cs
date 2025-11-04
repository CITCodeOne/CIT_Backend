using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using BusinessLayer.DTOs;

namespace WebServiceLayer.Controllers;

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
    public ActionResult<RatingDTO> GetRatingByPath(string uconst, string tconst)
    {
        // Validate input
        if (!int.TryParse(uconst, out int uconstParsed)) return BadRequest(new { message = "'uconst' must be a valid integer" });

        var rating = _mdb.Rating.GetRating(uconstParsed, tconst);
        if (rating == null) return NotFound(new { message = $"Rating for user '{uconst}' with title '{tconst}' not found" });
        return Ok(rating);
    }

    // GET: api/ratings?userId={uconst} or api/ratings?titleId={tconst}  "getting ratings by single part of composite key"
    [HttpGet]
    [ProducesResponseType(typeof(List<RatingDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<RatingDTO>> GetRating([FromQuery] string? userId, [FromQuery] string? titleId)
    {
        bool uconstProvided = !string.IsNullOrEmpty(userId);
        bool tconstProvided = !string.IsNullOrEmpty(titleId);
        // Validate input
        if (uconstProvided && tconstProvided) return BadRequest(new { message = "Provide either 'uconst' or 'tconst', not both" });
        if (!uconstProvided && !tconstProvided) return BadRequest(new { message = "Either 'uconst' or 'tconst' must be provided" });

        // Fetch by uconst
        if (uconstProvided)
        {
            if (!int.TryParse(userId, out int uconstParsed)) return BadRequest(new { message = "'uconst' must be a valid integer" });
            var ratingsByUser = _mdb.Rating.GetRatings(uconstParsed, null);
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

    // TODO: DELETE: api/ratings/{uconst}/{tconst}  "deleting a rating"
    // TODO: POST: api/ratings  "creating a new rating"

    // WARN: Arguably, we should look at more HTTP methods like PUT also.
}
