using Microsoft.AspNetCore.Mvc;
using DataService.Entities;
using DataService.DTOs;
using DataService.Data;
using Microsoft.EntityFrameworkCore;

namespace WebService.Controllers;
[ApiController]
[Route("api/genre")]
public class GenreController : ControllerBase
{
  private readonly GenreService service;

    public GenreController(CITContext context)
    {
        service = new GenreService(context);
    }

    [HttpGet] //GET: api/genre
    public async Task<ActionResult<List<GenreReferenceDTO>>> GetAllGenres()
    {
        var genres = await (from g in service.GetGenreSet()
                            select new GenreReferenceDTO(g.Id, g.Name))
                            .ToListAsync();
        if (genres == null)
        {
            return NotFound();
        }
        return Ok(genres);
    }

    [HttpGet("{gconst}")]
    public async Task<ActionResult<GenreFullDTO>> GetTitlesFromGenre(int gconst)
    {
        
        var genreTitles = await service.GetGenreSet()
                                       .Where(g => g.Id == gconst)
                                       .FirstOrDefaultAsync();
        if (genreTitles == null)
        {
            return NotFound();
        }
        return Ok(genreTitles);
    }
}