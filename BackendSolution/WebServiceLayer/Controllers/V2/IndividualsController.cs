using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using BusinessLayer.DTOs;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/individuals")]
public class IndividualsController : ControllerBase
{
    private readonly MdbService _mdb;

    public IndividualsController(MdbService mdbService)
    {
        _mdb = mdbService;
    }

    // GET: api/v2/individuals?page=1&pageSize=20
    [HttpGet]
    [ProducesResponseType(typeof(List<IndividualReferenceDTO>), StatusCodes.Status200OK)]
    public ActionResult<List<IndividualReferenceDTO>> GetIndividuals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var individuals = _mdb.Individual.ReferenceByPage(page, pageSize);
        return Ok(individuals);
    }

    // GET: api/v2/individuals/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IndividualFullDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IndividualFullDTO> GetIndividual(string id)
    {
        var individual = _mdb.Individual.FullById(id);

        if (individual == null)
            return NotFound(new { message = $"Individual with id '{id}' not found" });

        return Ok(individual);
    }
}
