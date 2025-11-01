namespace DataService.DTOs;

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