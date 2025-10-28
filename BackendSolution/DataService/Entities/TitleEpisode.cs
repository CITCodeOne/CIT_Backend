using System;
using System.Collections.Generic;

namespace DataService.Entities;

public partial class TitleEpisode
{
    public string? Tconst { get; set; }

    public string? Parenttconst { get; set; }

    public int? Seasonnumber { get; set; }

    public int? Episodenumber { get; set; }
}
