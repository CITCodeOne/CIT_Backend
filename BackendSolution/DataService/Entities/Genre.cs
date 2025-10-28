using System;
using System.Collections.Generic;

namespace DataService.Entities;

public partial class Genre
{
    public int Gconst { get; set; }

    public string? Gname { get; set; }

    public virtual ICollection<Title> Tconsts { get; set; } = new List<Title>();
}
