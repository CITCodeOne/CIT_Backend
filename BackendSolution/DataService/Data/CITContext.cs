using Microsoft.EntityFrameworkCore;
using DataService.Entities;
using DataService.Util;
using System.Text.Json;

namespace DataService.Data;

public class CITContext : DbContext
{
    public DbSet<Title> Titles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Search> Searches { get; set; }
    public DbSet<Visit> Visits { get; set; }

    private record DbConfig
    {
        public string? Host { get; set; }
        public string? Port { get; set; }
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
        var conn = $"Host={jsonSerialised.Host};Port={jsonSerialised.Port};Database={jsonSerialised.Database};Username={jsonSerialised.User};Password={jsonSerialised.Password}";
        optionsBuilder.UseNpgsql(conn);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Title>(b =>
        {
            b.ToTable("titles");
            b.Property(t => t.Id).HasColumnName("tconst");
            b.Property(t => t.Name).HasColumnName("title_name");
            b.Property(t => t.MediaType).HasColumnName("media_type").HasConversion(v => v.ToString(), v => (MediaType)Enum.Parse(typeof(MediaType), v));
            b.Property(t => t.AvgRating).HasColumnName("avg_rating");
            b.Property(t => t.NumVotes).HasColumnName("numvotes");
            b.Property(t => t.ReleaseDate).HasColumnName("release_date");
            b.Property(t => t.Adult).HasColumnName("is_adult");
            b.Property(t => t.StartYear).HasColumnName("start_year");
            b.Property(t => t.EndYear).HasColumnName("end_year");
            b.Property(t => t.Runtime).HasColumnName("runtime");
            b.Property(t => t.Poster).HasColumnName("poster");
            b.Property(t => t.PlotPre).HasColumnName("plot").HasComputedColumnSql("LEFT(plot, 25)", stored: false);
            modelBuilder.Entity<Title>().HasMany(t => t.Genres).WithMany(g => g.Titles).UsingEntity(j => j.ToTable("title_genres")); // Junction table
        });

        modelBuilder.Entity<Genre>(b =>
        {
            b.ToTable("genres");
            b.Property(x => x.Id).HasColumnName("genre_id");
            b.Property(x => x.Name).HasColumnName("genre_name");
        });

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("user_info");
            b.HasKey(u => u.Id);
            b.Property(u => u.Id).HasColumnName("uconst");
            b.Property(u => u.Name).HasColumnName("user_name");
            b.Property(u => u.Email).HasColumnName("email");
            b.Property(u => u.RegT).HasColumnName("time");
        });


        modelBuilder.Entity<UserAction>().UseTpcMappingStrategy(); // TPC maps each concrete type to its own table instead of using a single table for the hierarchy

        modelBuilder.Entity<Search>(b =>
        {
            b.ToTable("search_history");
            b.Property<int>("uconst");
            b.Property(s => s.time).HasColumnName("time");
            b.Property(s => s.SearchString).HasColumnName("search_string");

            b.HasKey("uconst", "time");

            b.HasOne(s => s.user)
                .WithMany()
                .HasForeignKey("uconst");
        });

        modelBuilder.Entity<Visit>(b =>
        {
            b.ToTable("visited_page");
            b.Property<int>("uconst");
            b.Property<int>("pconst");
            b.Property(v => v.time).HasColumnName("time");

            b.HasKey("uconst", "time");

            b.HasOne(v => v.user)
                .WithMany()
                .HasForeignKey("uconst");

            // We currently ignore the Page navigation to avoid conflicting inheritance mapping with Title/Individual
            b.Ignore(v => v.page);
        });
    }
}
