using Microsoft.AspNetCore.Mvc;
using BusinessLayer;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/pages")]
public class PageController : ControllerBase
{
    // Controller til at hente enkeltstående sider (artikler eller
    // undersider) i systemet. Den er ansvarlig for at finde og returnere
    // indhold baseret på en sides id.
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
        // Dette endpoint tager et pageId som tekst. Før vi forsøger at
        // finde siden, sikrer vi os at id'et kan omdannes til et heltal.
        // Hvis ikke, svarer vi med BadRequest fordi input ikke var gyldigt.
        if (!int.TryParse(pageId, out int pid)) return BadRequest(new { message = "PageId does not parse as an int" });

        // Hent siden fra forretningslaget. Hvis den ikke findes, returner
        // NotFound så klienten ved at den ønskede side ikke eksisterer.
        var page = _mdbService.Page.GetPageById(pid);
        if (page == null) return NotFound(new { message = "Page not found" });
        return Ok(page);
    }
}
