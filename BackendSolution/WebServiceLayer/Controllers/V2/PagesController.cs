using Microsoft.AspNetCore.Mvc;
using BusinessLayer;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/pages")]
public class PageController : ControllerBase
{
    private readonly MdbService _mdbService;
    private readonly IConfiguration _configuration;

    public PageController(MdbService mdbService, IConfiguration configuration)
    {
        _mdbService = mdbService;
        _configuration = configuration;
    }

    // GET api/v2/pages/{pageId}
    [HttpGet("{pageId}")]
    public IActionResult GetPageById(string pageId)
    {
        //Validate pageId is an integer
        if (!int.TryParse(pageId, out int pid)) return BadRequest(new { message = "PageId does not parse as an int" });

        var page = _mdbService.Page.GetPageById(pid);
        if (page == null) return NotFound(new { message = "Page not found" });
        return Ok(page);
    }
}
