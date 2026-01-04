using Microsoft.AspNetCore.Mvc;
using BusinessLayer;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/[controller]")]
public class HealthController : ControllerBase
{
    // Enkel "health check"-controller som bruges til at kontrollere at
    // webservicen kører og at den centrale service (`MdbService`) er
    // tilgængelig. 
    private readonly MdbService _mdbService;

    public HealthController(MdbService mdbService)
    {
        _mdbService = mdbService;
    }

    // GET: api/health
    [HttpGet]
    public ActionResult<string> GetHealth()
    {
        // Returnér en kort tekst som bekræfter at tjenesten svarer.
        // Dette endpoint returnerer altid en kort succesbesked hvis
        // applikationen er oppe.
        return Ok("WSL is running and the mdbService is available.");
    }
}


