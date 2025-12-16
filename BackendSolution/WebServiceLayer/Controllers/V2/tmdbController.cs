using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/tmdb")]
public class TmdbController : ControllerBase
{
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
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { message = "query parameter is required" });

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

        // Return raw JSON from TMDB
        return Content(content, "application/json");
    }

    // GET api/v2/tmdb/person/{id}?append=external_ids,images,combined_credits
    [HttpGet("person/{id}")]
    public async Task<IActionResult> GetPersonDetails(string id, [FromQuery] string? append)
    {
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

        return Content(content, "application/json");
    }

    // GET api/v2/tmdb/movie/posters?query=movie+name
    [HttpGet("movie/posters")]
    public async Task<IActionResult> GetMoviePosters([FromQuery] string? query)
    {
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

        // Parse JSON and extract posters
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

        return Ok(posters);
    }
}
