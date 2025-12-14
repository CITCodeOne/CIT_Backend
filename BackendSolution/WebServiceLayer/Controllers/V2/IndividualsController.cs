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

    // GET: api/v2/individuals/{id}/titles
    [HttpGet("{id}/titles")]
    [ProducesResponseType(typeof(List<TitlePreviewDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<List<TitlePreviewDTO>> GetIndividualTitles(string id)
    {
        // Verify individual exists
        var individual = _mdb.Individual.FullById(id);
        if (individual == null)
            return NotFound(new { message = $"Individual with id '{id}' not found" });
        var titles = _mdb.Individual.TitlesByIndividual(id);
        return Ok(titles);
    }

    // GET: api/v2/individuals/{id}/popular-actors
    [HttpGet("{id}/popular-actors")]
    [ProducesResponseType(typeof(List<IndividualFullDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<List<IndividualFullDTO>> GetPopularActors(string id)
    {
        if (string.IsNullOrEmpty(id) || (!id.StartsWith("tt") && !id.StartsWith("nm")))
            return BadRequest(new { message = "ID must start with 'tt' or 'nm'" });

        try
        {
            var actors = _mdb.Individual.GetPopularActors(id);
            return Ok(actors);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/v2/individuals/co-actors?actorName={name}
    [HttpGet("co-actors")]
    [ProducesResponseType(typeof(List<CoActorsDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<List<CoActorsDTO>> GetCoActors([FromQuery] string actorName)
    {
        if (string.IsNullOrEmpty(actorName))
            return BadRequest(new { message = "Actor name is required" });

        try
        {
            var coActors = _mdb.Individual.GetCoActors(actorName);
            return Ok(coActors);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/v2/individuals/search?name={name}
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<IndividualSearchResultDTO>), StatusCodes.Status200OK)]
    public ActionResult<List<IndividualSearchResultDTO>> SearchIndividuals([FromQuery] string name)
    {
        var results = _mdb.Individual.SearchIndividuals(name ?? "");
        return Ok(results);
    }
}
