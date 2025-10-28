using System;
using System.Collections.Generic;

namespace DataService.Entities;

public partial class Episode
{
    public string? Parenttconst { get; set; }

    public string Tconst { get; set; } = null!;

    public int? Snum { get; set; }

    public int? Epnum { get; set; }

    public virtual Title? ParenttconstNavigation { get; set; }
}
