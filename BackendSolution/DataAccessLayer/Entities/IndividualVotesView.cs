using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class IndividualVotesView
{
    public string Iconst { get; set; } = null!;

    public string? Name { get; set; }

    public short? BirthYear { get; set; }

    public short? DeathYear { get; set; }

    public double? NameRating { get; set; }

    public int? Pconst { get; set; }

    public int TotalVotes { get; set; }
}
