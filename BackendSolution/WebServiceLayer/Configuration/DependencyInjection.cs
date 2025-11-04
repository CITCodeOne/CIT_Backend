using AutoMapper;
using BusinessLayer.Mappings;
using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public static class DependencyInjection
{
    // Aggregates infrastructure + business registrations so Program.cs stays minimal.
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database provider and connection string are configured here (composition root)
        // Prefer ConnectionStrings:Default; otherwise build from flat dbconfig.json keys if present.
        var conn = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(conn))
        {
            var host = configuration["Host"] ?? configuration["Db:Host"] ?? configuration["DbConfig:Host"];
            var port = configuration["Port"] ?? configuration["Db:Port"] ?? configuration["DbConfig:Port"];
            var database = configuration["Database"] ?? configuration["Db:Database"] ?? configuration["DbConfig:Database"];
            var user = configuration["User"] ?? configuration["Db:User"] ?? configuration["DbConfig:User"];
            var password = configuration["Password"] ?? configuration["Db:Password"] ?? configuration["DbConfig:Password"];

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(host)) parts.Add($"Host={host}");
            if (!string.IsNullOrWhiteSpace(port)) parts.Add($"Port={port}");
            if (!string.IsNullOrWhiteSpace(database)) parts.Add($"Database={database}");
            if (!string.IsNullOrWhiteSpace(user)) parts.Add($"Username={user}");
            if (!string.IsNullOrWhiteSpace(password)) parts.Add($"Password={password}");
            conn = parts.Count > 0 ? string.Join(';', parts) : null;
        }

        services.AddDbContext<CITContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(conn))
            {
                options.UseNpgsql(conn);
            }
            // Centralize EF logging here
            options.EnableSensitiveDataLogging();
            options.LogTo(Console.WriteLine, LogLevel.Information);
        });

        // AutoMapper: scan BusinessLayer profiles
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // Application services
        services.AddScoped<ITitleService, TitleService>();

        return services;
    }
}
