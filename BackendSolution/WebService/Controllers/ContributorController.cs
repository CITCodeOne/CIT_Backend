using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Entities;
using DataService.DTOs;
using DataService.Data;

namespace WebService.Controllers;


/// Endpoints for querying contributors for titles and individuals.
[ApiController]
[Route("api/contributor")]
public class ContributorController : ControllerBase
{
  private readonly CITContext _context;
  private readonly IMapper _mapper;

  public ContributorController(CITContext context, IMapper mapper)
  {
    _context = context;
    _mapper = mapper;
  }

  /// Return contributors for a given title as full DTOs.
  [HttpGet("{tconst}")]
  public async Task<ActionResult<List<ContributorFullDTO>>> GetContributorsByTitle(string tconst)
  {
    var contributors = await _context.Contributors
      .Where(c => c.Tconst == tconst)
      .OrderBy(c => c.Priority)
      .ThenBy(c => c.Contribution)
      .ToListAsync();

    var dto = _mapper.Map<List<ContributorFullDTO>>(contributors);
    return Ok(dto);
  }

  /// Return contributors for a given title as basic DTOs (lighter payload).
  [HttpGet("{tconst}/basic")]
  public async Task<ActionResult<List<ContributorDTO>>> GetContributorsByTitleBasic(string tconst)
  {
    var contributors = await _context.Contributors
      .Where(c => c.Tconst == tconst)
      .OrderBy(c => c.Priority)
      .ThenBy(c => c.Contribution)
      .ToListAsync();

    var dto = _mapper.Map<List<ContributorDTO>>(contributors);
    return Ok(dto);
  }

  /// Return contributors for a given title as reference DTOs (IDs only).
  [HttpGet("{tconst}/ref")]
  public async Task<ActionResult<List<ContributorReferenceDTO>>> GetContributorsByTitleRef(string tconst)
  {
    var contributors = await _context.Contributors
      .Where(c => c.Tconst == tconst)
      .OrderBy(c => c.Priority)
      .ThenBy(c => c.Contribution)
      .ToListAsync();

    var dto = _mapper.Map<List<ContributorReferenceDTO>>(contributors);
    return Ok(dto);
  }

  /// Return a single contributor (full DTO) by individual ID.
  [HttpGet("individual/{iconst}")]
  public async Task<ActionResult<ContributorFullDTO>> GetContributorById(string iconst)
  {
    var contributor = await _context.Contributors
      .Where(c => c.Iconst == iconst)
      .FirstOrDefaultAsync();

    if (contributor == null) return NotFound();

    var dto = _mapper.Map<ContributorFullDTO>(contributor);
    return Ok(dto);
  }


  /// Return a single contributor (basic DTO) by individual ID.
  [HttpGet("individual/{iconst}/basic")]
  public async Task<ActionResult<ContributorDTO>> GetContributorByIdBasic(string iconst)
  {
    var contributor = await _context.Contributors
      .Where(c => c.Iconst == iconst)
      .FirstOrDefaultAsync();

    if (contributor == null) return NotFound();

    var dto = _mapper.Map<ContributorDTO>(contributor);
    return Ok(dto);
  }


  /// Return a single contributor (reference DTO) by individual ID.
  [HttpGet("individual/{iconst}/ref")]
  public async Task<ActionResult<ContributorReferenceDTO>> GetContributorByIdRef(string iconst)
  {
    var contributor = await _context.Contributors
      .Where(c => c.Iconst == iconst)
      .FirstOrDefaultAsync();

    if (contributor == null) return NotFound();

    var dto = _mapper.Map<ContributorReferenceDTO>(contributor);
    return Ok(dto);
  }


  /// Return a single contributor (full DTO) filtered by title and individual IDs.
  [HttpGet("title/{tconst}/individual/{iconst}")]
  public async Task<ActionResult<ContributorFullDTO>> GetContributorByTitleAndId(string tconst, string iconst)
  {
    var contributor = await _context.Contributors
      .Where(c => c.Tconst == tconst && c.Iconst == iconst)
      .FirstOrDefaultAsync();

    if (contributor == null) return NotFound();

    var dto = _mapper.Map<ContributorFullDTO>(contributor);
    return Ok(dto);
  }


  /// Return a single contributor (basic DTO) filtered by title and individual IDs.
  [HttpGet("title/{tconst}/individual/{iconst}/basic")]
  public async Task<ActionResult<ContributorDTO>> GetContributorByTitleAndIdBasic(string tconst, string iconst)
  {
    var contributor = await _context.Contributors
      .Where(c => c.Tconst == tconst && c.Iconst == iconst)
      .FirstOrDefaultAsync();

    if (contributor == null) return NotFound();

    var dto = _mapper.Map<ContributorDTO>(contributor);
    return Ok(dto);
  }

  /// Return a single contributor (reference DTO) filtered by title and individual IDs.
  [HttpGet("title/{tconst}/individual/{iconst}/ref")]
  public async Task<ActionResult<ContributorReferenceDTO>> GetContributorByTitleAndIdRef(string tconst, string iconst)
  {
    var contributor = await _context.Contributors
      .Where(c => c.Tconst == tconst && c.Iconst == iconst)
      .FirstOrDefaultAsync();

    if (contributor == null) return NotFound();

    var dto = _mapper.Map<ContributorReferenceDTO>(contributor);
    return Ok(dto);
  }

  /// Return all contributors as full DTOs.
  [HttpGet("all")]
  public async Task<ActionResult<List<ContributorFullDTO>>> GetAllContributors([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 50 : pageSize;
    pageSize = pageSize > 200 ? 200 : pageSize; // cap to prevent huge payloads

    var contributors = await _context.Contributors
      .AsNoTracking()
      .OrderBy(c => c.Tconst)
      .ThenBy(c => c.Iconst)
      .ThenBy(c => c.Priority)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync();

    var dto = _mapper.Map<List<ContributorFullDTO>>(contributors);
    return Ok(dto);
  }

  /// Return all contributors as basic DTOs (lighter payload).
  [HttpGet("all/basic")]
  public async Task<ActionResult<List<ContributorDTO>>> GetAllContributorsBasic([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 50 : pageSize;
    pageSize = pageSize > 200 ? 200 : pageSize;

    var contributors = await _context.Contributors
      .AsNoTracking()
      .OrderBy(c => c.Tconst)
      .ThenBy(c => c.Iconst)
      .ThenBy(c => c.Priority)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync();

    var dto = _mapper.Map<List<ContributorDTO>>(contributors);
    return Ok(dto);
  }

  /// Return all contributors as reference DTOs (IDs only).
  [HttpGet("all/ref")]
  public async Task<ActionResult<List<ContributorReferenceDTO>>> GetAllContributorsRef([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 50 : pageSize;
    pageSize = pageSize > 200 ? 200 : pageSize;

    var contributors = await _context.Contributors
      .AsNoTracking()
      .OrderBy(c => c.Tconst)
      .ThenBy(c => c.Iconst)
      .ThenBy(c => c.Priority)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync();

    var dto = _mapper.Map<List<ContributorReferenceDTO>>(contributors);
    return Ok(dto);
  }

  /// Get total count of contributors (for paging)
  [HttpGet("count")]
  public async Task<ActionResult<int>> GetContributorsCount()
  {
    var count = await _context.Contributors.CountAsync();
    return Ok(count);
  }
}
