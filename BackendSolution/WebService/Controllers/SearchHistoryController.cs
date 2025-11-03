using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Data;
using DataService.DTOs;

namespace WebService.Controllers;

[ApiController]
[Route("api/user/{userId}/searchhistory")]
public class SearchHistoryController : ControllerBase

{
    private readonly CITContext _context;
    private readonly IMapper _mapper;

    public SearchHistoryController(CITContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }


    /// Get all search history for a specific user, ordered by time descending (newest search first)
    [HttpGet]
    public async Task<ActionResult<List<SearchHistoryDTO>>> GetSearchHistory(int userId)
    {
        // User exists check
        var userExists = await _context.UserInfos
            .AnyAsync(u => u.Uconst == userId);

        if (!userExists)
        {
            return NotFound(new { message = $"User with ID {userId} not found." });
        }

        // Get search history entries for the user
        var searchHistory = await _context.SearchHistories
            .Where(sh => sh.Uconst == userId)
            .OrderByDescending(sh => sh.Time)
            .Select(sh => new SearchHistoryDTO
            {
                UserId = sh.Uconst,
                Time = sh.Time,
                SearchTerms = sh.SearchString ?? string.Empty
            })
            .ToListAsync();

        return Ok(searchHistory);
    }

    /// Delete a specific search history entry
    [HttpDelete]
    public async Task<IActionResult> DeleteSearchHistory(
        int userId,
        [FromQuery] DateTime time)
    {
        var searchHistory = await _context.SearchHistories
            .FirstOrDefaultAsync(sh => sh.Uconst == userId && sh.Time == time);

        if (searchHistory == null)
        {
            return NotFound(new
            {
                message = "Search history entry not found.",
                userId = userId,
                time = time.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            });
        }

        _context.SearchHistories.Remove(searchHistory);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// Clear all search history for a specific user
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearSearchHistory(int userId)
    {
        var searchHistories = await _context.SearchHistories
            .Where(sh => sh.Uconst == userId)
            .ToListAsync();

        if (!searchHistories.Any())
        {
            return NotFound(new { message = $"No search history found for user {userId}." });
        }

        _context.SearchHistories.RemoveRange(searchHistories);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}