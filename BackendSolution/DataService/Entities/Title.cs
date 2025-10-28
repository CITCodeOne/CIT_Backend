using System;
using System.Collections.Generic;

namespace DataService.Entities;

public partial class Title
{
    public string Tconst { get; set; } = null!;

    public string? TitleName { get; set; }

    public string? MediaType { get; set; }

    public double? AvgRating { get; set; }

    public int? Numvotes { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    public bool? IsAdult { get; set; }

    public short? StartYear { get; set; }

    public short? EndYear { get; set; }

    public TimeSpan? Runtime { get; set; }

    public string? Poster { get; set; }

    public string? Plot { get; set; }

    public virtual ICollection<Contributor> Contributors { get; set; } = new List<Contributor>();

    public virtual ICollection<Episode> Episodes { get; set; } = new List<Episode>();

    public virtual Page? Page { get; set; }

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<Wi> Wis { get; set; } = new List<Wi>();

    public virtual ICollection<Genre> Gconsts { get; set; } = new List<Genre>();
}
