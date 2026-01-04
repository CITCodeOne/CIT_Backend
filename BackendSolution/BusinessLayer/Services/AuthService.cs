using BusinessLayer.DTOs; // DTO-typer (Data Transfer Objects) bruges til at sende/afgrænse data mellem lagene
using DataAccessLayer.Data; // Context for databaseadgang
using DataAccessLayer.Entities; // Entitetsklasser der repræsenterer DB-tabeller

namespace BusinessLayer.Services;

// Service til håndtering af bruger-login og registrering.
// Forklaring for ikke-teknikere: denne klasse taler med databasen for at oprette
// brugere, tjekke adgangskoder og returnere de få oplysninger klienten har brug for.
public class AuthService
{
    // Database-kontekst: bruges til at hente og gemme brugeroplysninger
    private readonly CITContext _ctx;

    // Hashing-service: bruges til at lave sikre, ikke-læselige versioner af passwords
    private readonly Hashing _hashing;

    // Konstruktør: når AuthService oprettes, får den adgang til DB og hashing-funktionalitet
    public AuthService(CITContext ctx, Hashing hashing)
    {
        _ctx = ctx;
        _hashing = hashing;
    }

    // Registrerer en ny bruger.
    // - Input: `UserRegistrationDTO` indeholder oplysninger fra klienten (brugernavn, email, password)
    // - Output: `AuthUserDTO` med grundlæggende brugerinfo (brugernavn, id, rolle)
    // Forklaring: metoden validerer input, tjekker om brugernavn allerede findes,
    // hasher passwordet (vi gemmer aldrig det rene password), opretter brugeren i databasen
    // og returnerer en forenklet repræsentation af den nye bruger.
    public AuthUserDTO Register(UserRegistrationDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        // Vælg et brugernavn baseret på felter i DTO'en
        var username = DetermineUsername(dto);

        // Grundlæggende validering af de vigtigste felter
        ValidateRegistration(username, dto.Email, dto.Password);

        // Tjek om brugernavnet allerede findes i databasen
        if (_ctx.UserInfos.Any(u => u.UserName == username))
        {
            // Hvis brugeren allerede findes, kan vi ikke registrere igen
            throw new InvalidOperationException("User already exists");
        }

        // Hash password og få en salt-værdi som også gemmes
        var (hashedPwd, salt) = _hashing.Hash(dto.Password);

        // Opret en ny database-post (entitet) som repræsenterer brugeren internt
        var user = new UserInfo
        {
            UserName = username,
            UPassword = hashedPwd, // Vi gemmer kun den hash'ede adgangskode
            Email = dto.Email?.Trim(),
            Salt = salt, // Salt bruges ved verificering af password
            Role = "User",
            Time = DateTime.UtcNow
        };

        // Tilføj til konteksten og gem i databasen
        _ctx.UserInfos.Add(user);
        _ctx.SaveChanges();

        // Returnér en forenklet DTO med de felter klienten har brug for
        return BuildAuthUser(user);
    }

    // Logger en bruger ind.
    // Forklaring: kontrollerer at brugernavn og password er angivet,
    // finder brugeren i databasen, hasher det angivne password med den gemte salt
    // og sammenligner med den gemte hash. Returnerer brugerinfo hvis korrekt.
    public AuthUserDTO Authenticate(UserLoginDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Username and password are required");
        }

        var user = _ctx.UserInfos.FirstOrDefault(u => u.UserName == dto.Username.Trim());

        // Hvis brugeren ikke findes eller password ikke matcher, returner en fejl
        if (user == null || !_hashing.Verify(dto.Password, user.UPassword ?? string.Empty, user.Salt ?? string.Empty))
        {
            throw new InvalidOperationException("Invalid username or password");
        }

        return BuildAuthUser(user);
    }

    // Hjælpefunktion: vælg et passende brugernavn ud fra registreringsdata
    private static string DetermineUsername(UserRegistrationDTO dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Username))
        {
            return dto.Username.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            return dto.Name.Trim();
        }

        // Hvis intet brugernavn kan udledes, returner tom streng (så validering slår fejl senere)
        return string.Empty;
    }

    // Hjælpefunktion: tjek at nødvendige felter er til stede
    private static void ValidateRegistration(string username, string? email, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required", nameof(password));
        }
    }

    // Konverterer intern `UserInfo` entitet til en forenklet DTO til klienten
    private static AuthUserDTO BuildAuthUser(UserInfo user) => new()
    {
        UserId = user.Uconst,
        Username = user.UserName,
        Role = user.Role
    };
}
