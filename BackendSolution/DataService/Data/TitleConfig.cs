using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataService.Entities;

namespace DataService.Data;
public class TitleConfig : IEntityTypeConfiguration<Title>
{
  public void Configure(EntityTypeBuilder<Title> b)
  {
    b.ToTable("title");
    b.HasKey(t => t.Id);
    b.Property(t => t.Id).HasColumnName("tconst");
    b.Property(t => t.Name).HasColumnName("title_name");
    b.Property(t => t.MediaType).HasColumnName("media_type").HasConversion<string>();
    b.Property(t => t.AvgRating).HasColumnName("avg_rating");
    b.Property(t => t.NumVotes).HasColumnName("numvotes");
    b.Property(t => t.ReleaseDate).HasColumnName("release_date");
    b.Property(t => t.Adult).HasColumnName("is_adult");
    //b.Property(t => t.StartYear).HasColumnName("start_year");
    //b.Property(t => t.EndYear).HasColumnName("end_year");
    b.Property(t => t.Runtime).HasColumnName("runtime");
    b.Property(t => t.Poster).HasColumnName("poster");
    b.Property(t => t.PlotPre).HasColumnName("plot").HasComputedColumnSql("LEFT(plot, 25)", stored: false);
    
    b.HasMany(t => t.Genres)
     .WithMany(g => g.Titles)
     .UsingEntity(j => j.ToTable("title_genre")); // Junction table

    // One-to-Many: title -> Ratings
    //b.HasMany(t => t.Ratings).WithOne(r => r.Title);
  }
}
