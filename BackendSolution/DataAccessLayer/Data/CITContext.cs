using System;
using System.Collections.Generic;
using DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;

namespace DataAccessLayer.Data;

public partial class CITContext : DbContext
{
    public CITContext()
    {
    }

    public CITContext(DbContextOptions<CITContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActorTitleView> ActorTitleViews { get; set; }

    public virtual DbSet<Bookmark> Bookmarks { get; set; }

    public virtual DbSet<Contributor> Contributors { get; set; }

    public virtual DbSet<Episode> Episodes { get; set; }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Individual> Individuals { get; set; }

    public virtual DbSet<Page> Pages { get; set; }

    public virtual DbSet<Rating> Ratings { get; set; }

    public virtual DbSet<SearchHistory> SearchHistories { get; set; }

    public virtual DbSet<Title> Titles { get; set; }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    public virtual DbSet<VisitedPage> VisitedPages { get; set; }

    public virtual DbSet<Wi> Wis { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Provider and logging are configured in the application's composition root (WebServiceLayer).
        // Intentionally left blank to avoid file I/O and duplicated configuration here.
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActorTitleView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("actor_title_view", "mdb");

            entity.Property(e => e.Contribution)
                .HasMaxLength(50)
                .HasColumnName("contribution");
            entity.Property(e => e.Iconst)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("iconst");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasColumnName("name");
            entity.Property(e => e.Tconst)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("tconst");
            entity.Property(e => e.TitleName)
                .HasMaxLength(256)
                .HasColumnName("title_name");
        });

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => new { e.Uconst, e.Pconst }).HasName("bookmarks_pkey");

            entity.ToTable("bookmarks", "mdb");

            entity.Property(e => e.Uconst).HasColumnName("uconst");
            entity.Property(e => e.Pconst).HasColumnName("pconst");
            entity.Property(e => e.Time)
                .HasDefaultValueSql("now()")
                .HasColumnName("time");

            entity.HasOne(d => d.PconstNavigation).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.Pconst)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bookmarks_pconst_fkey");

            entity.HasOne(d => d.UconstNavigation).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.Uconst)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("bookmarks_uconst_fkey");
        });

        modelBuilder.Entity<Contributor>(entity =>
        {
            entity.HasKey(e => new { e.Tconst, e.Iconst, e.Contribution, e.Priority }).HasName("contributor_pkey");

            entity.ToTable("contributor", "mdb");

            entity.Property(e => e.Tconst)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("tconst");
            entity.Property(e => e.Iconst)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("iconst");
            entity.Property(e => e.Contribution)
                .HasMaxLength(50)
                .HasColumnName("contribution");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.Detail).HasColumnName("detail");

            entity.HasOne(d => d.IconstNavigation).WithMany(p => p.Contributors)
                .HasForeignKey(d => d.Iconst)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("contributor_iconst_fkey");

            entity.HasOne(d => d.TconstNavigation).WithMany(p => p.Contributors)
                .HasForeignKey(d => d.Tconst)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("contributor_tconst_fkey");
        });

        modelBuilder.Entity<Episode>(entity =>
        {
            entity.HasKey(e => e.Tconst).HasName("episode_pkey");

            entity.ToTable("episode", "mdb");

            entity.Property(e => e.Tconst)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("tconst");
            entity.Property(e => e.Epnum).HasColumnName("epnum");
            entity.Property(e => e.Parenttconst)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("parenttconst");
            entity.Property(e => e.Snum).HasColumnName("snum");

            entity.HasOne(d => d.ParenttconstNavigation).WithMany(p => p.Episodes)
                .HasForeignKey(d => d.Parenttconst)
                .HasConstraintName("episode_parenttconst_fkey");
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Gconst).HasName("genre_pkey");

            entity.ToTable("genre", "mdb");

            entity.Property(e => e.Gconst).HasColumnName("gconst");
            entity.Property(e => e.Gname).HasColumnName("gname");

            entity.HasMany(d => d.Tconsts).WithMany(p => p.Gconsts)
                .UsingEntity<Dictionary<string, object>>(
                    "TitleGenre",
                    r => r.HasOne<Title>().WithMany()
                        .HasForeignKey("Tconst")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("title_genre_tconst_fkey"),
                    l => l.HasOne<Genre>().WithMany()
                        .HasForeignKey("Gconst")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("title_genre_gconst_fkey"),
                    j =>
                    {
                        j.HasKey("Gconst", "Tconst").HasName("title_genre_pkey");
                        j.ToTable("title_genre", "mdb");
                        j.IndexerProperty<int>("Gconst").HasColumnName("gconst");
                        j.IndexerProperty<string>("Tconst")
                            .HasMaxLength(10)
                            .IsFixedLength()
                            .HasColumnName("tconst");
                    });
        });

        modelBuilder.Entity<Individual>(entity =>
        {
            entity.HasKey(e => e.Iconst).HasName("individual_pkey");

            entity.ToTable("individual", "mdb");

            entity.Property(e => e.Iconst)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("iconst");
            entity.Property(e => e.BirthYear).HasColumnName("birth_year");
            entity.Property(e => e.DeathYear).HasColumnName("death_year");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasColumnName("name");
            entity.Property(e => e.NameRating).HasColumnName("name_rating");
        });

        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasKey(e => e.Pconst).HasName("page_pkey");

            entity.ToTable("page", "mdb");

            entity.HasIndex(e => e.Iconst, "page_iconst_key").IsUnique();

            entity.HasIndex(e => e.Tconst, "page_tconst_key").IsUnique();

            entity.Property(e => e.Pconst).HasColumnName("pconst");
            entity.Property(e => e.Iconst)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("iconst");
            entity.Property(e => e.Tconst)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("tconst");

            entity.HasOne(d => d.IconstNavigation).WithOne(p => p.IndividualPage)
                .HasForeignKey<Page>(d => d.Iconst)
                .HasConstraintName("page_iconst_fkey");

            entity.HasOne(d => d.TconstNavigation).WithOne(p => p.TitlePage)
                .HasForeignKey<Page>(d => d.Tconst)
                .HasConstraintName("page_tconst_fkey");
        });

        modelBuilder.Entity<Rating>(entity =>
        {
            entity.HasKey(e => new { e.Uconst, e.Tconst }).HasName("rating_pkey");

            entity.ToTable("rating", "mdb");

            entity.Property(e => e.Uconst).HasColumnName("uconst");
            entity.Property(e => e.Tconst)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("tconst");
            entity.Property(e => e.Rating1).HasColumnName("rating");
            entity.Property(e => e.Time)
                .HasDefaultValueSql("now()")
                .HasColumnName("time");

            entity.HasOne(d => d.TconstNavigation).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.Tconst)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rating_tconst_fkey");

            entity.HasOne(d => d.UconstNavigation).WithMany(p => p.Ratings)
                .HasForeignKey(d => d.Uconst)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rating_uconst_fkey");
        });

        modelBuilder.Entity<SearchHistory>(entity =>
        {
            entity.HasKey(e => new { e.Uconst, e.Time }).HasName("search_history_pkey");

            entity.ToTable("search_history", "mdb");

            entity.Property(e => e.Uconst).HasColumnName("uconst");
            entity.Property(e => e.Time)
                .HasDefaultValueSql("now()")
                .HasColumnName("time");
            entity.Property(e => e.SearchString)
                .HasMaxLength(256)
                .HasColumnName("search_string");

            entity.HasOne(d => d.UconstNavigation).WithMany(p => p.SearchHistories)
                .HasForeignKey(d => d.Uconst)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("search_history_uconst_fkey");
        });

        modelBuilder.Entity<Title>(entity =>
        {
            entity.HasKey(e => e.Tconst).HasName("title_pkey");

            entity.ToTable("title", "mdb");

            entity.Property(e => e.Tconst)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("tconst");
            entity.Property(e => e.AvgRating).HasColumnName("avg_rating");
            entity.Property(e => e.EndYear).HasColumnName("end_year");
            entity.Property(e => e.IsAdult).HasColumnName("is_adult");
            entity.Property(e => e.MediaType).HasColumnName("media_type");
            entity.Property(e => e.Numvotes).HasColumnName("numvotes");
            entity.Property(e => e.Plot).HasColumnName("plot");
            entity.Property(e => e.Poster)
                .HasMaxLength(180)
                .HasColumnName("poster");
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
            entity.Property(e => e.Runtime).HasColumnName("runtime");
            entity.Property(e => e.StartYear).HasColumnName("start_year");
            entity.Property(e => e.TitleName)
                .HasMaxLength(256)
                .HasColumnName("title_name");
        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasKey(e => e.Uconst).HasName("user_info_pkey");

            entity.ToTable("user_info", "mdb");

            entity.HasIndex(e => e.UserName, "user_info_user_name_key").IsUnique();

            entity.Property(e => e.Uconst).HasColumnName("uconst");
            entity.Property(e => e.Email)
                .HasMaxLength(80)
                .HasColumnName("email");
            entity.Property(e => e.Time)
                .HasDefaultValueSql("now()")
                .HasColumnName("time");
            entity.Property(e => e.UPassword)
                .HasMaxLength(80)
                .HasColumnName("u_password");
            entity.Property(e => e.UserName)
                .HasMaxLength(80)
                .HasColumnName("user_name");
        });

        modelBuilder.Entity<VisitedPage>(entity =>
        {
            entity.HasKey(e => new { e.Uconst, e.Time }).HasName("visited_page_pkey");

            entity.ToTable("visited_page", "mdb");

            entity.Property(e => e.Uconst).HasColumnName("uconst");
            entity.Property(e => e.Time)
                .HasDefaultValueSql("now()")
                .HasColumnName("time");
            entity.Property(e => e.Pconst).HasColumnName("pconst");

            entity.HasOne(d => d.PconstNavigation).WithMany(p => p.VisitedPages)
                .HasForeignKey(d => d.Pconst)
                .HasConstraintName("visited_page_pconst_fkey");

            entity.HasOne(d => d.UconstNavigation).WithMany(p => p.VisitedPages)
                .HasForeignKey(d => d.Uconst)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("visited_page_uconst_fkey");
        });

        modelBuilder.Entity<Wi>(entity =>
        {
            entity.HasKey(e => new { e.Tconst, e.Word, e.Field }).HasName("wi_pkey");

            entity.ToTable("wi", "mdb");

            entity.Property(e => e.Tconst)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("tconst");
            entity.Property(e => e.Word).HasColumnName("word");
            entity.Property(e => e.Field)
                .HasMaxLength(1)
                .HasColumnName("field");
            entity.Property(e => e.Lexeme).HasColumnName("lexeme");

            entity.HasOne(d => d.TconstNavigation).WithMany(p => p.Wis)
                .HasForeignKey(d => d.Tconst)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_wi_tconst");
        });
        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
