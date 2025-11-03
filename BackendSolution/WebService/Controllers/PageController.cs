using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Entities;
using DataService.DTOs;
using DataService.Data;

namespace WebService.Controllers;

[ApiController]
[Route("api/page")]
public class PageController : ControllerBase
{
    private readonly CITContext _context;
    private readonly IMapper _mapper;

    public PageController(CITContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    //Get all page references
    [HttpGet("ref")]
    public async Task<ActionResult<List<PageReferenceDTO>>> GetAllPageReferences()
    {
        int pageSize = 100;

        var pages = await _context.Pages
                                .OrderBy(g => g.Pconst)
                                .Take(pageSize)
                                .ToListAsync();
        return Ok(_mapper.Map<List<PageReferenceDTO>>(pages));
    }
    //Get all full pages
    [HttpGet]
    public async Task<ActionResult<List<PageFullDTO>>> GetAllPages()
    {
        int pageSize = 100;

        var pages = await _context.Pages
                                .Include(p => p.TconstNavigation)
                                .Include(p => p.IconstNavigation)
                                .OrderBy(g => g.Pconst)
                                .Take(pageSize)
                                .ToListAsync();
        return Ok(_mapper.Map<List<PageFullDTO>>(pages));
    }
    //Get page reference from pconst
    [HttpGet("{pconst}/ref")]
    public async Task<ActionResult<PageReferenceDTO>> GetPageReferenceFromPconst(int pconst)
    {
        var page = await _context.Pages
                                   .Where(p => p.Pconst == pconst)
                                   .FirstOrDefaultAsync();
        if (page == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<PageReferenceDTO>(page));
    }
    //Get full page from pconst
    [HttpGet("{pconst}")]
    public async Task<ActionResult<PageFullDTO>> GetPageFromPconst(int pconst)
    {
        var page = await _context.Pages
                                .Include(p => p.TconstNavigation)
                                .Include(p => p.IconstNavigation)
                                .Where(p => p.Pconst == pconst)
                                .FirstOrDefaultAsync();
        if (page == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<PageFullDTO>(page));
    }
    //Get page reference from title
    [HttpGet("title/{tconst}/ref")]
    public async Task<ActionResult<PageReferenceDTO>> GetPageReferenceFromTitle(string tconst)
    {
        var page = await _context.Pages
            .Include(p => p.TconstNavigation)
            .Where(p => p.Tconst == tconst)
            .FirstOrDefaultAsync();
        if (page == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<PageReferenceDTO>(page));
    }
    //Get full page from title
    [HttpGet("title/{tconst}")]
    public async Task<ActionResult<PageFullDTO>> GetPageFromTitle(string tconst)
    {
        var page = await _context.Pages
            .Include(p => p.TconstNavigation)
            .Where(p => p.Tconst == tconst)
            .FirstOrDefaultAsync();
        if (page == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<PageFullDTO>(page));
    }
    //Get page reference from individual
    [HttpGet("individual/{iconst}/ref")]
    public async Task<ActionResult<PageReferenceDTO>> GetPageReferenceFromIndividual(string iconst)
    {
        var page = await _context.Pages
            .Include(p => p.IconstNavigation)
            .Where(p => p.Iconst == iconst)
            .FirstOrDefaultAsync();
        if (page == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<PageReferenceDTO>(page));
    }
    //Get full page from individual
    [HttpGet("individual/{iconst}")]
    public async Task<ActionResult<PageFullDTO>> GetPageFromIndividual(string iconst)
    {
        var page = await _context.Pages
            .Include(p => p.IconstNavigation)
            .Where(p => p.Iconst == iconst)
            .FirstOrDefaultAsync();
        if (page == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<PageFullDTO>(page));
    }
}
