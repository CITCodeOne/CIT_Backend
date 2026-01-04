using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BusinessLayer;
using BusinessLayer.DTOs;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/users")]
public class UsersController : ControllerBase
{
    // Denne controller samler alle endpoints relateret til brugere.
    // Hvert endpoint (metode) svarer til en handling som klienten kan
    // bede serveren om, fx "hent bruger", "tilføj bogmærke" eller
    // "opdater profilbillede".
    private readonly MdbService _mdbService;

    public UsersController(MdbService mdbService)
    {
        // `MdbService` er et centralt service-objekt der kan udføre
        // forskellige operationer mod forretningslogikken og databasen.
        // Controlleren bruger dette for at holde selve controller-koden
        // enkel og uden direkte databasekode.
        _mdbService = mdbService;
    }

    // GET: api/v2/users/{userId}
    [HttpGet("{userId}")]
    public IActionResult GetUser(int userId)
    {
        // Hent en brugers offentlige oplysninger ud fra deres ID.
        // Hvis brugeren ikke findes, svarer vi med "NotFound" som betyder
        // at den forespurgte ressource ikke kunne findes.
        var user = _mdbService.User.GetUserById(userId);
        if (user == null) return NotFound(new { message = $"User with id '{userId}' not found" });
        return Ok(user);
    }

    // BOOKMARKS

    // GET: api/v2/users/{userId}/bookmarks
    [HttpGet("{userId}/bookmarks")]
    public IActionResult GetBookmarks(int userId)
    {
        // Returnerer en liste over de sider denne bruger har gemt som
        // bogmærker. Hvis der ikke er nogen, returnerer vi en tom liste.
        var bookmarks = _mdbService.Bookmark.GetBookmarksByUser(userId);
        if (bookmarks.Count == 0) return Ok(new List<BookmarkDTO>());
        return Ok(bookmarks);
    }

    // GET: api/v2/users/{userId}/bookmarks/{pageId}
    [HttpGet("{userId}/bookmarks/{pageId}")]
    public IActionResult GetBookmark(int userId, int pageId)
    {
        // Hent et enkelt bogmærke for en bruger og en side. Hvis det
        // ikke findes, svarer vi med NotFound.
        var bookmark = _mdbService.Bookmark.GetBookmark(userId, pageId);
        if (bookmark == null) return NotFound(new { message = $"Bookmark for user '{userId}' with page '{pageId}' not found" });
        return Ok(bookmark);
    }

    // POST: api/v2/users/{userId}/bookmarks
    [HttpPost("{userId}/bookmarks")]
    [Authorize]
    public IActionResult CreateBookmark(int userId, CreateBookmarkDTO bookmarkCreateDto)
    {
        // Dette endpoint kræver at brugeren er logget ind. Vi henter
        // brugerens id fra det medsendte token (claim "uid"). Hvis tokenet
        // mangler eller ikke matcher den bruger der forsøger at lave
        // handlingen, afvises anmodningen.
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });

        // Forhindre at en bruger forsøger at ændre en anden brugers data.
        if (authenticatedUserId != userId)
            return Forbid();

        try
        {
            // Forsøg at tilføje et nyt bogmærke. Hvis det allerede findes,
            // returnerer vi Conflict for at sige at ressource allerede findes.
            var added = _mdbService.Bookmark.AddBookmark(userId, bookmarkCreateDto.PageId);
            if (added == null)
                return Conflict(new { message = "Bookmark already exists" });

            return Ok(new { message = "Bookmark added", bookmark = added });
        }
        catch (InvalidOperationException ex)
        {
            // Hvis noget er galt med oplysningerne, returneres en fejl.
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE: api/v2/users/{userId}/bookmarks/{pageId}
    [HttpDelete("{userId}/bookmarks/{pageId}")]
    [Authorize]
    public IActionResult DeleteBookmark(int userId, int pageId)
    {
        // Slet et bogmærke. Samme sikkerhedstjek som ved oprettelse:
        // token skal angive samme bruger.
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });

        if (authenticatedUserId != userId)
            return Forbid();

        var removed = _mdbService.Bookmark.RemoveBookmark(userId, pageId);
        if (!removed)
            return NotFound(new { message = "Bookmark not found" });

        return Ok(new { message = "Bookmark removed" });
    }

    // RATINGS

    // GET: api/v2/users/{userId}/ratings
    [HttpGet("{userId}/ratings")]
    public IActionResult GetRatings(int userId)
    {
        // Hent alle bedømmelser som en bruger har afgivet. Hvis ingen
        // bedømmelser findes, returneres en tom liste.
        var ratings = _mdbService.Rating.GetRatings(userId, null);
        if (ratings.Count == 0)
            return Ok(new List<RatingDTO>());

        return Ok(ratings);
    }

    // GET: api/v2/users/{userId}/ratings/{titleId}
    [HttpGet("{userId}/ratings/{titleId}")]
    public IActionResult GetRating(int userId, string titleId)
    {
        // Hent en enkelt bedømmelse for en specifik titel.
        var rating = _mdbService.Rating.GetRating(userId, titleId);
        if (rating == null)
            return NotFound(new { message = $"Rating for user '{userId}' with title '{titleId}' not found" });

        return Ok(rating);
    }

    // POST: api/v2/users/{userId}/ratings
    [HttpPost("{userId}/ratings")]
    [Authorize]
    public IActionResult CreateRating(int userId, CreateRatingDTO ratingCreateDto)
    {
        // Opret en ny bedømmelse. Samme token-sikkerhed som tidligere.
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });

        if (authenticatedUserId != userId)
            return Forbid();

        try
        {
            // Kald et asynkront opkald for at gemme bedømmelsen. `.Wait()`
            // bruges her for at vente på at operationen fuldføres før
            // vi svarer klienten.
            _mdbService.Rating.RateAsync(userId, ratingCreateDto.TitleId, ratingCreateDto.Rating, ratingCreateDto.ReviewText).Wait();
            return Ok(new { message = "Rating submitted" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT: api/v2/users/{userId}/ratings/{titleId}
    [HttpPut("{userId}/ratings/{titleId}")]
    [Authorize]
    public IActionResult UpdateRating(int userId, string titleId, UpdateRatingDTO ratingUpdateDto)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });

        if (authenticatedUserId != userId) return Forbid();

        // Før vi opdaterer en bedømmelse, tjekker vi at den findes. Hvis ikke,
        // svarer vi med NotFound. Hvis den findes, opdaterer vi bedømmelsen.
        var existingRating = _mdbService.Rating.GetRating(userId, titleId);
        if (existingRating == null)
            return NotFound(new { message = $"Rating for user '{userId}' with title '{titleId}' not found" });

        try
        {
            _mdbService.Rating.RateAsync(userId, titleId, ratingUpdateDto.Rating, ratingUpdateDto.ReviewText).Wait();
            return Ok(new { message = "Rating updated" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        // NOTE: This currently is not entirely RESTful since this requires a rating even if we just want to update the review text.
    }

    // DELETE: api/v2/users/{userId}/ratings/{titleId}
    [HttpDelete("{userId}/ratings/{titleId}")]
    [Authorize]
    public IActionResult DeleteRating(int userId, string titleId)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });

        if (authenticatedUserId != userId) return Forbid();

        // Tjek at bedømmelsen findes før vi sletter. Herefter slettes den.
        var existingRating = _mdbService.Rating.GetRating(userId, titleId);
        if (existingRating == null)
            return NotFound(new { message = $"Rating for user '{userId}' with title '{titleId}' not found" });

        try
        {
            _mdbService.Rating.DeleteRatingAsync(userId, titleId).Wait();
            return Ok(new { message = "Rating deleted" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/v2/users/{userId}/visits
    [HttpGet("{userId}/visits")]
    [Authorize]
    public IActionResult GetVisits(int userId)
    {
        // Hent hvilke sider brugeren har besøgt. Returner en tom liste hvis
        // ingen besøg findes.
        var visits = _mdbService.Visit.GetVisitsByUserId(userId);
        if (visits.Count == 0)
            return Ok(new List<VisitedPageDTO>());

        return Ok(visits);
    }

    // POST: api/v2/users/{userId}/visits
    [HttpPost("{userId}/visits")]
    [Authorize]
    public IActionResult AddVisit(int userId, CreateVisitedPageDTO visitedPageCreateDto)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });

        if (authenticatedUserId != userId)
            return Forbid();

        try
        {
            // Tilføj et besøg til brugerens historik. Dette kan bruges til
            // at vise seneste besøg eller anbefalinger.
            var added = _mdbService.Visit.AddVisitedPage(userId, visitedPageCreateDto.PageId);
            return Ok(new { message = "Visited page added", visit = added });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PROFILE IMAGE
    // GET: api/v2/users/{userId}/profile-image
    [HttpGet("{userId}/profile-image")]
    public IActionResult GetProfileImage(int userId)
    {
        // Hent en brugers profilbillede kodet som tekst (fx base64). Hvis
        // brugeren ikke har et billede, svarer vi med NotFound.
        var image = _mdbService.User.GetProfileImage(userId);
        if (image == null) return NotFound(new { message = $"User '{userId}' has no profile image" });
        return Ok(new UserProfileImageDTO { UserId = userId, ProfileImage = image });
    }

    // PUT: api/v2/users/{userId}/profile-image
    [HttpPut("{userId}/profile-image")]
    [Authorize]
    public IActionResult UpsertProfileImage(int userId, UpdateProfileImageDTO dto)
    {
        var uidClaim = User.FindFirst("uid")?.Value;
        if (string.IsNullOrEmpty(uidClaim) || !int.TryParse(uidClaim, out var authenticatedUserId))
            return Unauthorized(new { message = "User id missing from token" });
        if (authenticatedUserId != userId) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.ImageBase64))
            return BadRequest(new { message = "ImageBase64 cannot be empty" });

        // Gem eller opdater brugerens profilbillede. `ImageBase64` skal
        // indeholde billeddata i tekstform. Hvis brugeren ikke findes,
        // returnerer vi NotFound.
        var saved = _mdbService.User.SetProfileImage(userId, dto.ImageBase64);
        if (!saved) return NotFound(new { message = $"User '{userId}' not found" });

        return Ok(new { message = "Profile image saved" });
    }
}
