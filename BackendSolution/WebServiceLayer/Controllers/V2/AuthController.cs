using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using BusinessLayer.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/auth")]
public class AuthController : ControllerBase
{
    // Denne controller håndterer brugerautentificering: oprettelse af brugere (signup)
    // og login.
    private readonly MdbService _mdbService;
    private readonly IConfiguration _configuration;

    public AuthController(MdbService mdbService, IConfiguration configuration)
    {
        // Konstruktoren får to ting ind (via afhængighedsinjektion):
        // 1) Et service-objekt (`MdbService`) som har metoder til at tale med
        //    forretningslogikken og databasen (fx oprette og godkende brugere).
        // 2) Konfiguration (`IConfiguration`) som indeholder hemmelige nøgler
        //    og andre indstillinger hentet fra appsettings eller miljøet.
        _mdbService = mdbService;
        _configuration = configuration;
    }

    [HttpPost("signup")]
    public IActionResult SignUp(UserRegistrationDTO model)
    {
        try
        {
            // Når en ny bruger vil oprettes, modtager vi et `model`-objekt med
            // oplysninger som brugernavn og kodeord. Her kaldes forretningslaget
            // for at oprette brugeren i databasen.
            var createdUser = _mdbService.Auth.Register(model);
            // Returnerer en simpel besked som bekræfter oprettelsen og viser
            // det valgte brugernavn. `Ok` betyder at anmodningen lykkedes.
            return Ok(new { message = "User created successfully", username = createdUser.Username });
        }
        catch (ArgumentException ex)
        {
            // Hvis input er forkert eller mangler noget, returnerer vi
            // `BadRequest` med en forklaring. Dette fortæller klienten at
            // noget var galt med de sendte data.
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // Andre fejl i oprettelsesprocessen håndteres også her.
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public IActionResult Login(UserLoginDTO model)
    {
        try
        {
            // Her forsøger vi at logge brugeren ind ved at lade forretningslaget
            // kontrollere brugernavn og kodeord. Hvis det lykkes, får vi et
            // `authUser`-objekt med oplysninger om brugeren.
            var authUser = _mdbService.Auth.Authenticate(model);

            // "Claims" er små udsagn om brugeren, fx deres brugernavn,
            // rolle og id. Disse bruges senere i et digitalt bevis (token),
            // så serveren kan kende brugeren uden at gemme deres kodeord.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, authUser.Username),
                new Claim(ClaimTypes.Role, authUser.Role),
                new Claim("uid", authUser.UserId.ToString())
            };

            // Hent en hemmelig nøgle fra konfigurationen. Denne nøgle bruges
            // til at lave et sikkert token. Hvis nøglen mangler, stopper vi
            // og rapporterer en fejl, for så kan vi ikke lave sikre tokens.
            var secret = _configuration.GetSection("Auth:Secret").Value
                ?? throw new InvalidOperationException("Auth:Secret is not configured");

            // Konverter den hemmelige tekst til en nøgle-objekt som kan bruges
            // til at signere tokenet (det beviser at tokenet kommer fra os).
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            // Opret de nødvendige oplysninger til at signere tokenet.
            // Signaturen sikrer at tokenet ikke er blevet ændret.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Her opretter vi selve tokenet (et lille digitalt bevis) som
            // indeholder brugerens "claims", en udløbstid og en signatur.
            // Udløbstiden betyder at brugeren skal logge ind igen senere.
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(4),
                signingCredentials: creds
            );

            // Konverter token-objektet til en tekststreng (JWT), som kan sendes
            // tilbage til klienten og bruges ved fremtidige kald til API'et.
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // Returnér brugernavn og det nye token. Klienten gemmer dette
            // og sender det med i efterfølgende anmodninger for at blive
            // genkendt.
            return Ok(new { username = authUser.Username, token = jwt });
        }
        catch (ArgumentException ex)
        {
            // Hvis noget i login-data er forkert (fx manglende felter),
            // forklarer vi kort hvorfor det fejlede.
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException)
        {
            // Hvis brugernavn/kodeord ikke matcher, giver vi en generel
            // besked om at login mislykkedes uden at afsløre detaljer.
            return BadRequest("Invalid username or password");
        }
    }
}
