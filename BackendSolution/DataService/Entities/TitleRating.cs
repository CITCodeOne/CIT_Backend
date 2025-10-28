using System;
using System.Collections.Generic;

namespace DataService.Entities;

public partial class TitleRating
{
    public string? Tconst { get; set; }

    public decimal? Averagerating { get; set; }

    public int? Numvotes { get; set; }
}
