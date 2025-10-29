using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Entities;
using DataService.DTOs;
using DataService.Data;

namespace WebService.Controllers;

[ApiController]
[Route("api/rating")]
public class RatingController : ControllerBase
{
  private readonly CITContext _context;
  private readonly IMapper _mapper;

  public RatingController(CITContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  // GET: api/rating/user/{userId}

  // Fetches all ratings made by a specific user, including title details
  [HttpGet("user/{userId}")]
  public async Task<ActionResult<List<RatingWithTitleDTO>>> GetUserRatings(int userId)
  {
    var ratings = await _context.Ratings
      .Include(r => r.TconstNavigation)
      .Where(r => r.Uconst == userId)
      .OrderByDescending(r => r.Time)
      .ToListAsync();

    return Ok(_mapper.Map<List<RatingWithTitleDTO>>(ratings));
  }

  // GET: api/rating/title/{titleId}
  // Fetches all ratings for a specific title, including user details
  [HttpGet("title/{titleId}")]
  public async Task<ActionResult<List<RatingWithUserDTO>>> GetTitleRatings(string titleId)
  {
    var ratings = await _context.Ratings
      .Include(r => r.UconstNavigation)
      .Where(r => r.Tconst == titleId)
      .OrderByDescending(r => r.Time)
      .ToListAsync();

    return Ok(_mapper.Map<List<RatingWithUserDTO>>(ratings));
  }

  // GET: api/rating/{userId}/{titleId}

  // Fetches a specific rating made by a user for a title
  [HttpGet("{userId}/{titleId}")]
  public async Task<ActionResult<RatingDTO>> GetRating(int userId, string titleId)
  {
    var rating = await _context.Ratings
      .FirstOrDefaultAsync(r => r.Uconst == userId && r.Tconst == titleId);

    if (rating == null)
    {
      return NotFound();
    }

    return Ok(_mapper.Map<RatingDTO>(rating));
  }

  // POST: api/rating

  // Creates or updates a rating for a title by a user
  [HttpPost]
  public async Task<ActionResult<RatingDTO>> CreateOrUpdateRating(CreateRatingDTO createDto, [FromQuery] int userId)
  {
    try
    {
      await _context.Database.ExecuteSqlRawAsync(
        "SELECT mdb.rate({0}, {1}, {2})",
        userId,
        createDto.TitleId,
        createDto.Rating
      );

      var rating = await _context.Ratings
        .FirstOrDefaultAsync(r => r.Uconst == userId && r.Tconst == createDto.TitleId);

      if (rating == null)
      {
        return StatusCode(500, "Rating was created but could not be retrieved");
      }

      return Ok(_mapper.Map<RatingDTO>(rating));
    }
    catch (Exception ex)
    {
      if (ex.InnerException?.Message.Contains("User does not exist") == true)
      {
        return NotFound("User not found");
      }
      if (ex.InnerException?.Message.Contains("Movie does not exist") == true)
      {
        return NotFound("Title not found");
      }
      if (ex.InnerException?.Message.Contains("Rating must always be between") == true)
      {
        return BadRequest("Rating must be between 1 and 10");
      }

      return StatusCode(500, $"An error occurred: {ex.Message}");
    }
  }

  // DELETE: api/rating/{userId}/{titleId}

  // Deletes a specific rating made by a user for a title
  [HttpDelete("{userId}/{titleId}")]
  public async Task<ActionResult> DeleteRating(int userId, string titleId)
  {
    try
    {
      await _context.Database.ExecuteSqlRawAsync(
        "SELECT mdb.delete_rating({0}, {1})",
        userId,
        titleId
      );

      return NoContent();
    }
    catch (Exception ex)
    {
      if (ex.InnerException?.Message.Contains("User does not exist") == true)
      {
        return NotFound("User not found");
      }
      if (ex.InnerException?.Message.Contains("Movie does not exist") == true)
      {
        return NotFound("Title not found");
      }
      if (ex.InnerException?.Message.Contains("Rating does not exist") == true)
      {
        return NotFound("Rating not found");
      }

      return StatusCode(500, $"An error occurred: {ex.Message}");
    }
  }

  // GET: api/rating/title/{titleId}/average

  // Fetches the average rating and total number of ratings for a specific title
  [HttpGet("title/{titleId}/average")]
  public async Task<ActionResult<object>> GetAverageRating(string titleId)
  {
    var title = await _context.Titles
      .FirstOrDefaultAsync(t => t.Tconst == titleId);

    if (title == null)
    {
      return NotFound("Title not found");
    }

    return Ok(new
    {
      titleId,
      averageRating = Math.Round(title.AvgRating ?? 0, 1),
      totalRatings = title.Numvotes ?? 0
    });
  }
}
