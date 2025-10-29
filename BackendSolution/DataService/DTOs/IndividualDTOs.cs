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