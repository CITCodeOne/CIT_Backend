using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Entities;
using DataService.DTOs;
using DataService.Data;

namespace WebService.Controllers;

[ApiController]
[Route("api/genre")]
public class GenreController : ControllerBase
{
    private readonly CITContext _context;
    private readonly IMapper _mapper;

    public GenreController(CITContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    //Get all genres
    [HttpGet]
    public async Task<ActionResult<List<GenreDTO>>> GetAllGenres()
    {
        var genres = await _context.Genres
                                   .OrderBy(g => g.Gconst)
                                   .ToListAsync();
        return Ok(_mapper.Map<List<GenreDTO>>(genres));
    }
    //Get genre from gconst
    [HttpGet("{gconst}")]
    public async Task<ActionResult<GenreDTO>> GetGenreFromGconst(int gconst)
    {
        var genre = await _context.Genres
                                       .Where(g => g.Gconst == gconst)
                                       .FirstOrDefaultAsync();
        if (genre == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<GenreDTO>(genre));
    }
}
