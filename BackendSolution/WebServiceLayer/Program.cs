using Microsoft.EntityFrameworkCore; // Importerer Entity Framework Core, som bruges til at arbejde med relationelle databaser i .NET
using DataAccessLayer.Data; // Importerer projektets namespace hvor `CITContext` (DbContext) er defineret
using BusinessLayer; // Importerer forretningslaget (services, helpers osv.)
using BusinessLayer.Mappings; // Importerer AutoMapper-mappingprofilen
using BusinessLayer.Services; // Importerer specifikke services fra forretningslaget
using System.Text.Json; // Importerer JSON-serialisering / deserialisering
using System.Text; // Importerer tekst/behandlingshjælp (fx Encoding)
using Microsoft.AspNetCore.Authentication.JwtBearer; // Importerer JWT Bearer authentication handler
using Microsoft.IdentityModel.Tokens; // Importerer typer til tokenvalidering og sikkerhedsnøgler


var builder = WebApplication.CreateBuilder(args); // Opretter en `WebApplicationBuilder` vha. CLI-argumenter (`args`) — bygger konfiguration, services og host

builder.Services.AddOpenApi(); // Registrerer OpenAPI (Swagger) service til automatisk API-dokumentation
builder.Services.AddControllers(); // Tilføjer understøttelse for MVC-controller-baserede endpoints (API controllers)
builder.Services.AddSingleton<Hashing>(); // Registrerer `Hashing` som en singleton (én instans i hele applikationens levetid)
builder.Services.AddCors(opt => // Konfigurerer CORS (Cross-Origin Resource Sharing)
{
    opt.AddPolicy("FrontendPolicy", policy => // Definerer en named CORS-policy kaldet "FrontendPolicy"
    {
        policy.WithOrigins( // Tillader kun requests fra disse origins (frontend udviklingsservere)
            "http://localhost:5173",
            "https://localhost:5173")
              .AllowAnyHeader() // Tillader alle HTTP-headers i anmodninger
              .AllowAnyMethod(); // Tillader alle HTTP-metoder (GET, POST, PUT, DELETE osv.)
    });
});

// JWT authentication konfiguration
// Kilde: https://github.com/bulskov/CIT_2025_Authentication (angivet som reference)
var secret = builder.Configuration.GetSection("Auth:Secret").Value // Læser den hemmelige nøgle fra konfiguration
    ?? throw new InvalidOperationException("Auth:Secret configuration is missing"); // Smider en fejl hvis nøglen ikke findes

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // Aktiverer authentication middleware og sætter standardscheme til JWT Bearer
    .AddJwtBearer(opt => // Konfigurerer JWT Bearer-specifikke indstillinger
    {
        opt.TokenValidationParameters = new TokenValidationParameters // Indstiller hvordan JWT tokens valideres
        {
            ValidateIssuerSigningKey = true, // Aktivér validering af signeringsnøglen
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), // Opretter en symmetrisk sikkerhedsnøgle baseret på `secret` (UTF8 bytes)
            ValidateIssuer = false, // Ikke kræve validering af issuer (kan sættes true i strengere setups)
            ValidateAudience = false, // Ikke kræve validering af audience
            ClockSkew = TimeSpan.Zero // Ingen tolereret tidsforskydning ved token udløb (bruges ofte til at udligne små tidsforskelle)
        };
    });

// Indlæs databasekonfiguration fra dbconfig.json (placeret i ContentRootPath)
var dbConfigPath = Path.Combine(builder.Environment.ContentRootPath, "dbconfig.json"); // Sammenkæder sti til dbconfig.json i applikationens rodmappe
var dbConfigJson = File.ReadAllText(dbConfigPath); // Læser hele filens indhold som en JSON-string
var dbConfig = JsonSerializer.Deserialize<Dictionary<string, string>>(dbConfigJson) // Deserialiserer JSON til en simpel key/value dictionary
    ?? throw new InvalidOperationException("Failed to load database configuration from dbconfig.json"); // Smider hvis deserialisering mislykkes

// Byg en connection string til PostgreSQL (Npgsql). Bemærk: bruger `User` som database-navn ifølge eksisterende konfiguration
var connectionString = $"Host={dbConfig["Host"]};Port={dbConfig["Port"]};Database={dbConfig["User"]};Username={dbConfig["User"]};Password={dbConfig["Password"]}";

builder.Services.AddDbContext<CITContext>(options => // Registrerer `CITContext` (DbContext) i DI containeren
    options.UseNpgsql(connectionString)); // Konfigurerer DbContext til at bruge Npgsql (Postgres) med den konstruerede connection string

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly); // Registrerer AutoMapper og tilføjer mappings fra `MappingProfile` assembly

builder.Services.AddScoped<MdbService>(); // Registrerer `MdbService` med Scoped lifetime (én instans per HTTP-request)
// Bruges til TMDB API kald
builder.Services.AddHttpClient(); // Registrerer fabriksservice til at lave `HttpClient`-instanser (bruges til eksterne HTTP-kald)

var app = builder.Build(); // Bygger `WebApplication` instansen ud fra builder (indlæser konfiguration, services og middleware pipeline)

if (app.Environment.IsDevelopment()) // Tjekker om vi kører i udviklingsmiljø
{
    app.MapOpenApi(); // Mapper/OpenAPI (Swagger) endpoints kun i udvikling for API-dokumentation
}

app.UseHttpsRedirection(); // Middleware: omdirigerer HTTP til HTTPS
app.UseCors("FrontendPolicy"); // Middleware: aktiverer den tidligere definerede CORS-policy
app.UseAuthentication(); // Middleware: aktiverer authentication (JWT validering på requests)
app.UseAuthorization(); // Middleware: aktiverer authorization (policies/roles)
app.MapControllers(); // Mapper controller endpoints ind i routing systemet

app.Run(); // Starter web-applikationen og begynder at lytte efter HTTP-requests

// INFO: `public partial class` gør Program-klassen tilgængelig for testprojekter.
// Uden denne deklaration kompileres Program-klassen ofte som intern (internal) og er dermed ikke tilgængelig fra andre assemblies.
// For integrationstests ønsker test-projektet at kunne starte applikationen programmatisk, derfor udsætter vi Program-klassen som `public partial`.
public partial class Program { }
