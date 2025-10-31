using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Entities;
using DataService.DTOs;
using DataService.Data;
using AutoMapper.QueryableExtensions; // For ProjectTo in GetTop10

namespace WebService.Controllers;

[ApiController]
[Route("api/individual")]
public class IndividualController : ControllerBase

{
    private readonly CITContext _context;
    private readonly IMapper _mapper;

    public IndividualController(CITContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }





    // Endpoint to get full details of an individual (by tconst)
    [HttpGet("{tconst}")]
    public async Task<ActionResult<IndividualFullDTO>> GetIndividual(string tconst)
    {
        var individual = await _context.Individuals
            .FirstOrDefaultAsync(i => i.Iconst == tconst);

        if (individual == null)
        {
            return NotFound("Individual not found.");
        }

        return Ok(_mapper.Map<IndividualFullDTO>(individual));
    }

    // Endpoint to get reference details of an individual (by tconst)
    [HttpGet("reference/{tconst}")]
    public async Task<ActionResult<IndividualReferenceDTO>> GetReference(string tconst)
    {
        var individual = await _context.Individuals
            .FirstOrDefaultAsync(i => i.Iconst == tconst);

        if (individual == null)
        {
            return NotFound("No references for individual " + tconst + " found.");
        }

        return Ok(_mapper.Map<IndividualReferenceDTO>(individual));
    }

    // Endpoint to get top 10 individuals - sorted by name but we might include rating?
    [HttpGet("top10")]
    public async Task<ActionResult<List<IndividualReferenceDTO>>> GetTop10([FromQuery] string? orderBy = "name")
    {
        var query = _context.Individuals.AsQueryable();

        query = orderBy?.ToLower() switch
        {
        "name" => query.OrderBy(i => i.Name),
            /*
            "rating" => query
                .Include(i => i.Ratings)
                .OrderByDescending(i => i.Ratings.Any() ? i.Ratings.Average(r => r.AverageRating) : 0),
            "rating_desc" => query
                .Include(i => i.Ratings)
                .OrderByDescending(i => i.Ratings.Any() ? i.Ratings.Average(r => r.AverageRating) : 0),
            */
            _ => query.OrderBy(i => i.Name) // default
           
        };

        var individuals = await query
            .Take(10)
            .ProjectTo<IndividualReferenceDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();

        if (!individuals.Any())
        {
            return NotFound(new { message = "No individuals found." });
        }

        return Ok(individuals);
    }


}