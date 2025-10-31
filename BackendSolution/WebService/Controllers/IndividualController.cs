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
            return NotFound(new { message = "Individual reference not found." });
        }

        return Ok(_mapper.Map<IndividualReferenceDTO>(individual));
    }



    // Endpoint to get list of individuals who contributed to a specific title
    [HttpGet("bytitle/{tconst}")]
    public async Task<ActionResult<IndividualsByTitleDTO>> GetByTitle(string tconst)
    {
        var title = await _context.Titles
            .Where(t => t.Tconst == tconst)
            .Select(t => new IndividualsByTitleDTO
            {
                Title = new TitleReferenceDTO
                {
                    Id = t.Tconst,
                    Name = t.TitleName ?? "Unknown"
                },
                Individuals = t.Contributors
                    .Select(c => new IndividualReferenceDTO
                    {
                        Id = c.Iconst,
                        Name = c.IconstNavigation.Name ?? "Unknown"
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (title == null)
        {
            return NotFound(new { message = $"Title with ID '{tconst}' not found." });
        }

        return Ok(title);
    }

    // Endpoint to search individuals by name with paging
    [HttpGet("search")]
    public async Task<ActionResult<List<IndividualReferenceDTO>>> Search([FromQuery] string? name, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(name)) // if search term is null, empty, or whitespace
        {
            return Ok(new List<IndividualReferenceDTO>());  // return empty list
        }

        if (page < 1)
            page = 1;

        if (pageSize < 1 || pageSize > 20)
            pageSize = 20;

        // Perform filtering, sorting, and paging directly in the database
        var individuals = await _context.Individuals
            .Where(i => i.Name != null && EF.Functions.Like(i.Name, $"{name}%")) // Use EF.Functions.Like for StartsWith (E.G starts with "name")
            .OrderBy(i => i.Name) // Sort in DB
            .Skip((page - 1) * pageSize) // Skip previous pages
            .Take(pageSize) // Take only current page
            .Select(i => new IndividualReferenceDTO
            {
                Id = i.Iconst,
                Name = i.Name ?? "Unknown"
            })
            .ToListAsync();

        return Ok(individuals);
    }

}