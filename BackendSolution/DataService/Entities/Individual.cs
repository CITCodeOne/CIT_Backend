using System;
using System.Collections.Generic;

namespace DataService.Entities;

public partial class Individual
{
    public string Iconst { get; set; } = null!;

    public string? Name { get; set; }

    public short? BirthYear { get; set; }

    public short? DeathYear { get; set; }

    public double? NameRating { get; set; }

    public virtual ICollection<Contributor> Contributors { get; set; } = new List<Contributor>();

    public virtual Page? Page { get; set; }
}
