using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Entities;
using DataService.DTOs;
using DataService.Data;

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
    [HttpGet("{tconst}")]
    public async Task<ActionResult<IndividualFullDTO>> Get(string tconst)
    {
        var individual = await _context.Individuals
            .FirstOrDefaultAsync(i => i.Iconst == tconst);

        if (individual == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<IndividualFullDTO>(individual));
    }

    [HttpGet("reference/{tconst}")]
    public async Task<ActionResult<IndividualReferenceDTO>> GetReference(string tconst)
    {
        var individual = await _context.Individuals
            .FirstOrDefaultAsync(i => i.Iconst == tconst);

        if (individual == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<IndividualReferenceDTO>(individual));
    }
}