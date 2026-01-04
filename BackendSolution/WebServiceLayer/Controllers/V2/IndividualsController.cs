using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using BusinessLayer.DTOs;
using BusinessLayer.Parameters;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/individuals")]
public class IndividualsController : ControllerBase
{
    // Controller for personer (individuelle personer som skuespillere,
    // instruktører osv.). Her kan klienten søge efter personer, hente
    // populære personer, se hvilke titler en person er med i og finde
    // medspillere (co-actors). Kommentarerne forklarer hvad hvert
    // endpoint gør i almindelige vendinger.
    private readonly MdbService _mdb;

    public IndividualsController(MdbService mdbService)
    {
        _mdb = mdbService;
    }

    // GET api/v2/individuals?params
    [HttpGet]
    [ProducesResponseType(typeof(List<IndividualReferenceDTO>), StatusCodes.Status200OK)]
    public ActionResult<List<IndividualReferenceDTO>> GetIndividuals([FromQuery] IndividualSearchParameters parameters)
    {
        // Søg efter personer med forskellige filtre og paginering. Vi
        // sikrer at sideinddelingen har fornuftige standardværdier og
        // returnerer derefter resultatlisten.
        if (parameters.Page < 1) parameters.Page = 1;
        if (parameters.PageSize < 1 || parameters.PageSize > 100) parameters.PageSize = 20;

        var individuals = _mdb.Individual.SearchIndividuals(parameters);
        return Ok(individuals);
    }

    // INFO: Depricated
    // REMOVE BEFORE FLIGHT
    //
    // // GET: api/v2/individuals?page=1&pageSize=20
    // [HttpGet]
    // [ProducesResponseType(typeof(List<IndividualReferenceDTO>), StatusCodes.Status200OK)]
    // public ActionResult<List<IndividualReferenceDTO>> GetIndividuals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    // {
    //     if (page < 1) page = 1;
    //     if (pageSize < 1 || pageSize > 100) pageSize = 20;
    //
    //     var individuals = _mdb.Individual.ReferenceByPage(page, pageSize);
    //     return Ok(individuals);
    // }

    // GET: api/v2/individuals/popular?page=1&pageSize=20
    [HttpGet("popular")]
    [ProducesResponseType(typeof(List<IndividualReferenceDTO>), StatusCodes.Status200OK)]
    public ActionResult<List<IndividualReferenceDTO>> GetPopularIndividuals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // Returnér de mest populære personer (baseret på stemmer). Resultatet
        // er pagineret så klienten kan hente flere sider.
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var individuals = _mdb.Individual.GetMostPopularIndividualsByVotes(page, pageSize);
        return Ok(individuals);
    }

    // GET: api/v2/individuals/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IndividualFullDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IndividualFullDTO> GetIndividual(string id)
    {
        // Hent fuld information om en person, fx biografi, billeder og
        // kendte titler. Hvis personen ikke findes, svarer vi med NotFound.
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
        // Returnér en liste over titler som personen medvirker i. Først
        // tjekker vi at personen findes, ellers returnerer vi NotFound.
        var individual = _mdb.Individual.FullById(id);
        if (individual == null)
            return NotFound(new { message = $"Individual with id '{id}' not found" });
        var titles = _mdb.Individual.TitlesByIndividual(id);
        return Ok(titles);
    }

    // TODO: This is not really working in accordance with RESTful principles, needs rethinking
    // GET: api/v2/individuals/{id}/popular-actors
    [HttpGet("{id}/popular-actors")]
    [ProducesResponseType(typeof(List<IndividualFullDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<List<IndividualFullDTO>> GetPopularActors(string id)
    {
        // Forsøg at finde populære medspillere til en given titel eller
        // person; dette endpoint forventer id'er med bestemte præfikser
        // (fx 'tt' for titler eller 'nm' for navne). Hvis id'et er forkert
        // formateret, svarer vi med BadRequest.
        if (string.IsNullOrEmpty(id) || (!id.StartsWith("tt") && !id.StartsWith("nm")))
            return BadRequest(new { message = "ID must start with 'tt' or 'nm'" });

        try
        {
            var actors = _mdb.Individual.GetPopularActors(id);
            return Ok(actors);
        }
        catch (Exception ex)
        {
            // Hvis noget går galt i forretningslaget, returnerer vi en
            // BadRequest med fejlbeskrivelsen.
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET: api/v2/individuals/search?name={name}
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<IndividualSearchResultDTO>), StatusCodes.Status200OK)]
    public ActionResult<List<IndividualSearchResultDTO>> SearchIndividuals([FromQuery] string name)
    {
        // Simpel søgning efter personer baseret på navn eller funktion.
        // Returnerer en liste af søgeresultater.
        var results = _mdb.Individual.SearchIndividualsByFunction(name ?? "");
        return Ok(results);
    }

    // GET: api/v2/individuals/co-actors?name=Tom%20Hanks
    [HttpGet("co-actors")]
    [ProducesResponseType(typeof(List<CoActorDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<List<CoActorDTO>> GetCoActors([FromQuery] string name)
    {
        // Find medspillere (co-actors) ved at give et navn. Hvis navnet ikke
        // er angivet, svarer vi med BadRequest.
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "Query parameter 'name' is required" });

        try
        {
            var results = _mdb.Individual.FindCoActors(name);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
