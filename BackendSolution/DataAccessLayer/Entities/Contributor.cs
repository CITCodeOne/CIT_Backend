using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Contributor
{
    public string Tconst { get; set; } = null!;

    public string Iconst { get; set; } = null!;

    public string Contribution { get; set; } = null!;

    public string? Detail { get; set; }

    public int Priority { get; set; }

    public virtual Individual IconstNavigation { get; set; } = null!;

    public virtual Title TconstNavigation { get; set; } = null!;
}
