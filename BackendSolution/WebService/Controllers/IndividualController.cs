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

    // Endpoint to get top individuals - sorted by name (max 20)
    [HttpGet("top")]
    public async Task<ActionResult<List<IndividualReferenceDTO>>> GetTop([FromQuery] int limit = 10) // 10 by default
    {
        if (limit > 20) // show max 20 individuals
            limit = 20;

        var individuals = await _context.Individuals
            .OrderBy(i => i.Name)
            .Take(limit)
            .ProjectTo<IndividualReferenceDTO>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Ok(individuals);
    }


}