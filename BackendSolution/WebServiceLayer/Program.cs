using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Data;
using BusinessLayer;
using BusinessLayer.Mappings;
using BusinessLayer.Services;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSingleton<Hashing>();

// JWT Authentication configuration
// Source: https://github.com/bulskov/CIT_2025_Authentication
var secret = builder.Configuration.GetSection("Auth:Secret").Value
    ?? throw new InvalidOperationException("Auth:Secret configuration is missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

// Load database configuration from dbconfig.json
var dbConfigPath = Path.Combine(builder.Environment.ContentRootPath, "dbconfig.json");
var dbConfigJson = File.ReadAllText(dbConfigPath);
var dbConfig = JsonSerializer.Deserialize<Dictionary<string, string>>(dbConfigJson)
    ?? throw new InvalidOperationException("Failed to load database configuration from dbconfig.json");

var connectionString = $"Host={dbConfig["Host"]};Port={dbConfig["Port"]};Database={dbConfig["User"]};Username={dbConfig["User"]};Password={dbConfig["Password"]}";

builder.Services.AddDbContext<CITContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddScoped<MdbService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// INFO: "public partial class" makes Program class accessible to tests.
// Otherwise, the test project cannot access the Program class to start the application for integration tests
// This has a longer explanation, but the underlying idea is that the program.cs file is compiled into an internal class by default
// and thus not accessible from other assemblies (like the test project).
// Therefore, we declare it as a public partial class here, such that the test project can access it.
public partial class Program { }
