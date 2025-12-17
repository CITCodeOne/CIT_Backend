namespace BusinessLayer.DTOs;

public class IndividualFullDTO
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public short? BirthYear { get; set; }
    public short? DeathYear { get; set; }
    public double? NameRating { get; set; }
}

public class IndividualReferenceDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? PageId { get; set; }
}

public class IndividualsByTitleDTO
{
    public TitleReferenceDTO Title { get; set; } = new();
    public List<IndividualReferenceDTO> Individuals { get; set; } = new();
}

public class CoActorsDTO
{
    public string Iconst { get; set; } = string.Empty;
    public string Primaryname { get; set; } = string.Empty;
    public long Co_Count { get; set; }
}

public class IndividualReferenceWithTotalVotesDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TotalVotes { get; set; }
    public double? NameRating { get; set; }
    public short? BirthYear { get; set; }
    public short? DeathYear { get; set; }
    public int? PageId { get; set; }
}

public class IndividualSearchResultDTO
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Contribution { get; set; } = string.Empty;
    public string TitleName { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
}
