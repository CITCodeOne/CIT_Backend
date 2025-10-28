using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataService.Entities;

namespace DataService.Data;
public class GenreConfig : IEntityTypeConfiguration<Genre>
{
  public void Configure(EntityTypeBuilder<Genre> b)
  {
    b.ToTable("genre");
    b.HasKey(g => g.Id);
    b.Property(g => g.Id).HasColumnName("gconst");
    b.Property(g => g.Name).HasColumnName("gname");

    b.HasMany(g => g.Titles)
     .WithMany(t => t.Genres)
     .UsingEntity(j => j.ToTable("title_genre"));
     
    /*
    b.HasMany(g => g.Titles).WithMany(t => t.Genres)
                            .UsingEntity(j => j.ToTable("title_genre")
                            .HasOne(g => g.Titles).WithMany()
                            .HasForeignKey(g => g.Id),
                            j => j.HasOne(t => t.Genres).WithMany()
                            .HasForeignKey(t => t.Id)
                            ); // Junction table
                            */
  }
}
