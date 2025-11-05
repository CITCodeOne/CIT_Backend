using Microsoft.AspNetCore.Mvc;
using BusinessLayer;

namespace WebServiceLayer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly MdbService _mdbService;

    public HealthController(MdbService mdbService)
    {
        _mdbService = mdbService;
    }

    // GET: api/health
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public ActionResult<string> GetHealth()
    {
        // Simple health check endpoint
        return Ok("WSL is running and the mdbService is available.");
    }
}

