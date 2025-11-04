// Helper is in global namespace to avoid namespace resolution issues

var builder = WebApplication.CreateBuilder(args);
// Load optional dbconfig.json so DI can construct the connection string if ConnectionStrings is not used
builder.Configuration.AddJsonFile("dbconfig.json", optional: true, reloadOnChange: true);
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Single aggregator for infra + business registrations
builder.Services.AddApplicationServices(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
