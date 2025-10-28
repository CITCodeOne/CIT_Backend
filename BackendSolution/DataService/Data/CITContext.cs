using Microsoft.EntityFrameworkCore;
using DataService.Entities;
using DataService.Util;
using System.Text.Json;
using System.Reflection;

namespace DataService.Data;

public class CITContext : DbContext
{
    public DbSet<Title> Titles => Set<Title>();
    public DbSet<Genre> Genres => Set<Genre>();

    private record DbConfig
    {
        public string? Host { get; set; }
        public string? Database { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
        optionsBuilder.EnableSensitiveDataLogging();
        var json = File.ReadAllText("dbconfig.json");
        var jsonSerialised = JsonSerializer.Deserialize<DbConfig>(json)!;
        var conn = $"Host={jsonSerialised.Host}; Database={jsonSerialised.Database};Username={jsonSerialised.User};Password={jsonSerialised.Password}";
        optionsBuilder.UseNpgsql(conn);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("mdb");
        modelBuilder.Entity<Title>(new TitleConfig().Configure);
        modelBuilder.Entity<Genre>(new GenreConfig().Configure);
    }
}