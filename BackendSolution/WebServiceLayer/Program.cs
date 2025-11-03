using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Data;
using BusinessLayer;
using BusinessLayer.Mappings;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

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
app.UseAuthorization();
app.MapControllers();

app.Run();
