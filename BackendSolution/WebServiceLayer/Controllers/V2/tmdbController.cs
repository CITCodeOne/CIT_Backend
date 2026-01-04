using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/tmdb")]
public class TmdbController : ControllerBase
{
    // Controlleren videreformidler forespørgsler til TheMovieDB (TMDB) API.
    // Den fungerer som et mellemled mellem vores app og den eksterne
    // TMDB-tjeneste: modtager søgninger fra klienten, henter data fra
    // TMDB og returnerer det til klienten. Kommentarerne her forklarer
    // hvad hvert endpoint gør i almindeligt sprog.
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _tmdbApiKey = string.Empty;

    public TmdbController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
    {
        _httpClientFactory = httpClientFactory;

        // Load API key from tmdbConfig.json located in the content root
        try
        {
            var cfgPath = System.IO.Path.Combine(env.ContentRootPath, "tmdbConfig.json");
            if (System.IO.File.Exists(cfgPath))
            {
                var json = System.IO.File.ReadAllText(cfgPath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("tmdbApiKey", out var keyEl))
                    // Læs den hemmelige API-nøgle som bruges til at spørge TMDB.
                    // Hvis filen mangler eller nøglen ikke findes, vil `_tmdbApiKey`
                    // forblive tom, og de endpoints der har brug for nøglen
                    // vil returnere en fejlkode.
                    _tmdbApiKey = keyEl.GetString() ?? string.Empty;
            }
        }
        catch
        {
            _tmdbApiKey = string.Empty;
        }
    }

    // GET api/v2/tmdb/person?query=actor+name
    [HttpGet("person")]
    public async Task<IActionResult> SearchPerson([FromQuery] string? query)
    {
        // Søger efter personer (skuespillere, instruktører osv.) i TMDB.
        // `query` er søgeteksten fra klienten. Hvis den ikke er angivet,
        // svarer vi med BadRequest som betyder at klienten sendte en
        // ugyldig anmodning.
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "query parameter is required" });

        // Hvis API-nøglen ikke er sat op, kan vi ikke kontakte TMDB,
        // og returnerer derfor en intern serverfejl (500).
        if (string.IsNullOrWhiteSpace(_tmdbApiKey))
            return StatusCode(500, new { message = "TMDB API key not configured" });

        var url = "https://api.themoviedb.org/3/search/person?query=" + Uri.EscapeDataString(query);
        var client = _httpClientFactory.CreateClient();

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tmdbApiKey);
        req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage resp;
        try
        {
            resp = await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = "Error contacting TMDB", detail = ex.Message });
        }

        var content = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            return StatusCode((int)resp.StatusCode, content);

        // Returnér det rå JSON-svar fra TMDB til klienten. Det betyder at
        // vi sender den data, TMDB gav os, uden stor forandring.
        return Content(content, "application/json");
    }

    // GET api/v2/tmdb/person/{id}?append=external_ids,images,combined_credits
    [HttpGet("person/{id}")]
    public async Task<IActionResult> GetPersonDetails(string id, [FromQuery] string? append)
    {
        // Hent detaljer om en person ud fra deres TMDB-id. `append` kan
        // angive ekstra information som billeder eller eksterne id'er.
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { message = "id is required" });

        if (string.IsNullOrWhiteSpace(_tmdbApiKey))
            return StatusCode(500, new { message = "TMDB API key not configured" });

        var appendPart = string.IsNullOrWhiteSpace(append) ? "external_ids,images,combined_credits" : append;
        var url = $"https://api.themoviedb.org/3/person/{Uri.EscapeDataString(id)}?append_to_response={Uri.EscapeDataString(appendPart)}";

        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tmdbApiKey);
        req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage resp;
        try
        {
            resp = await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = "Error contacting TMDB", detail = ex.Message });
        }

        var content = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            return StatusCode((int)resp.StatusCode, content);

        // Returnér TMDB's svar direkte som JSON.
        return Content(content, "application/json");
    }

    // GET api/v2/tmdb/movie/posters?query=movie+name
    [HttpGet("movie/posters")]
    public async Task<IActionResult> GetMoviePosters([FromQuery] string? query)
    {
        // Søg efter film og udtræk link til plakatbilleder. Returnerer en
        // liste med titel og URL til plakaten.
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "query parameter is required" });

        if (string.IsNullOrWhiteSpace(_tmdbApiKey))
            return StatusCode(500, new { message = "TMDB API key not configured" });

        var url = "https://api.themoviedb.org/3/search/movie?query=" + Uri.EscapeDataString(query);
        var client = _httpClientFactory.CreateClient();

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tmdbApiKey);
        req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage resp;
        try
        {
            resp = await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = "Error contacting TMDB", detail = ex.Message });
        }

        var content = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            return StatusCode((int)resp.StatusCode, content);

        // Parse JSON og udtræk plakater: vi læser TMDB's svar og bygger
        // en forenklet liste med titler og direkte URL'er til billederne.
        using var doc = JsonDocument.Parse(content);
        var results = doc.RootElement.GetProperty("results");

        var posters = new List<object>();
        foreach (var movie in results.EnumerateArray())
        {
            var title = movie.GetProperty("title").GetString();
            var posterPath = movie.GetProperty("poster_path").GetString();
            if (!string.IsNullOrEmpty(posterPath))
            {
                var posterUrl = $"https://image.tmdb.org/t/p/w500{posterPath}";
                posters.Add(new { title, posterUrl });
            }
        }

        // Returnér en simpel liste med plakater som klienten nemt kan
        // vise i UI'et.
        return Ok(posters);
    }

    // GET api/v2/tmdb/movie/{id}?append=credits,images
    [HttpGet("movie/{id}")]
    public async Task<IActionResult> GetMovieDetails(string id, [FromQuery] string? append)
    {
        // Hent detaljer for en film inkl. mulighed for at angive ekstra
        // data via `append` (fx credits og billeder).
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { message = "id is required" });

        if (string.IsNullOrWhiteSpace(_tmdbApiKey))
            return StatusCode(500, new { message = "TMDB API key not configured" });

        var appendPart = string.IsNullOrWhiteSpace(append) ? "credits,images" : append;
        var url = $"https://api.themoviedb.org/3/movie/{Uri.EscapeDataString(id)}?append_to_response={Uri.EscapeDataString(appendPart)}";

        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tmdbApiKey);
        req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage resp;
        try
        {
            resp = await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = "Error contacting TMDB", detail = ex.Message });
        }

        var content = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            return StatusCode((int)resp.StatusCode, content);

        return Content(content, "application/json");
    }

    // GET api/v2/tmdb/movie/search?query=...
    [HttpGet("movie/search")]
    public async Task<IActionResult> SearchMovie([FromQuery] string? query)
    {
        // Generisk film-søgning, sender TMDB's svar direkte tilbage.
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "query parameter is required" });

        if (string.IsNullOrWhiteSpace(_tmdbApiKey))
            return StatusCode(500, new { message = "TMDB API key not configured" });

        var url = "https://api.themoviedb.org/3/search/movie" + Request.QueryString;
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tmdbApiKey);
        req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage resp;
        try
        {
            resp = await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = "Error contacting TMDB", detail = ex.Message });
        }

        var content = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            return StatusCode((int)resp.StatusCode, content);

        return Content(content, "application/json");
    }

    // GET api/v2/tmdb/tv/search?query=...
    [HttpGet("tv/search")]
    public async Task<IActionResult> SearchTv([FromQuery] string? query)
    {
        // Søg efter tv-serier. Returnerer TMDB's svar direkte.
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "query parameter is required" });

        if (string.IsNullOrWhiteSpace(_tmdbApiKey))
            return StatusCode(500, new { message = "TMDB API key not configured" });

        var url = "https://api.themoviedb.org/3/search/tv" + Request.QueryString;
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tmdbApiKey);
        req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage resp;
        try
        {
            resp = await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = "Error contacting TMDB", detail = ex.Message });
        }

        var content = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            return StatusCode((int)resp.StatusCode, content);

        return Content(content, "application/json");
    }

    // GET api/v2/tmdb/person/search?query=...
    [HttpGet("person/search")]
    public async Task<IActionResult> SearchPersonAlt([FromQuery] string? query)
    {
        // Alternativ person-søgning som genbruger TMDBs standard-søgeendpoint.
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "query parameter is required" });

        if (string.IsNullOrWhiteSpace(_tmdbApiKey))
            return StatusCode(500, new { message = "TMDB API key not configured" });

        var url = "https://api.themoviedb.org/3/search/person" + Request.QueryString;
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tmdbApiKey);
        req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage resp;
        try
        {
            resp = await client.SendAsync(req);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = "Error contacting TMDB", detail = ex.Message });
        }

        var content = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            return StatusCode((int)resp.StatusCode, content);

        return Content(content, "application/json");
    }

    // GET api/v2/tmdb/movie/{id}/similar
    [HttpGet("movie/{id}/similar")]
    public async Task<IActionResult> GetMovieSimilar(string id)
    {
        // Find lignende film til den angivne film-id og returnér TMDBs svar.
        if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { message = "id is required" });
        if (string.IsNullOrWhiteSpace(_tmdbApiKey)) return StatusCode(500, new { message = "TMDB API key not configured" });

        var url = $"https://api.themoviedb.org/3/movie/{Uri.EscapeDataString(id)}/similar" + Request.QueryString;
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tmdbApiKey);
        req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage resp;
        try { resp = await client.SendAsync(req); } catch (Exception ex) { return StatusCode(502, new { message = "Error contacting TMDB", detail = ex.Message }); }
        var content = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode, content);
        return Content(content, "application/json");
    }

    // GET api/v2/tmdb/tv/{id}/similar
    [HttpGet("tv/{id}/similar")]
    public async Task<IActionResult> GetTvSimilar(string id)
    {
        // Find lignende tv-serier for en given tv-id.
        if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { message = "id is required" });
        if (string.IsNullOrWhiteSpace(_tmdbApiKey)) return StatusCode(500, new { message = "TMDB API key not configured" });

        var url = $"https://api.themoviedb.org/3/tv/{Uri.EscapeDataString(id)}/similar" + Request.QueryString;
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tmdbApiKey);
        req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage resp;
        try { resp = await client.SendAsync(req); } catch (Exception ex) { return StatusCode(502, new { message = "Error contacting TMDB", detail = ex.Message }); }
        var content = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode, content);
        return Content(content, "application/json");
    }

    // GET api/v2/tmdb/find?imdbId=ttXXXXX
    [HttpGet("find")]
    public async Task<IActionResult> FindByImdb([FromQuery] string? imdbId)
    {
        // Slå TMDB-oplysninger op ved hjælp af et IMDb-id. Bruges når
        // vi allerede kender filmens eller seriens IMDb-nummer.
        if (string.IsNullOrWhiteSpace(imdbId)) return BadRequest(new { message = "imdbId parameter is required" });
        if (string.IsNullOrWhiteSpace(_tmdbApiKey)) return StatusCode(500, new { message = "TMDB API key not configured" });

        var url = $"https://api.themoviedb.org/3/find/{Uri.EscapeDataString(imdbId)}?external_source=imdb_id";
        var client = _httpClientFactory.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tmdbApiKey);
        req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage resp;
        try { resp = await client.SendAsync(req); } catch (Exception ex) { return StatusCode(502, new { message = "Error contacting TMDB", detail = ex.Message }); }
        var content = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode) return StatusCode((int)resp.StatusCode, content);
        return Content(content, "application/json");
    }
}
