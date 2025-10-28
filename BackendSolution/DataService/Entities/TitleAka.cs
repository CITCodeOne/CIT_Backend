using System;
using System.Collections.Generic;

namespace DataService.Entities;

public partial class TitleAka
{
    public string? Titleid { get; set; }

    public int? Ordering { get; set; }

    public string? Title { get; set; }

    public string? Region { get; set; }

    public string? Language { get; set; }

    public string? Types { get; set; }

    public string? Attributes { get; set; }

    public bool? Isoriginaltitle { get; set; }
}
