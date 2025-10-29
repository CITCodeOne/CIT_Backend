namespace DataService.DTOs;

public class ContributorFullDTO
{
  public string Tconst { get; set; } = null!;
  public string Iconst { get; set; } = null!;
  public string Contribution { get; set; } = null!;
  public string? Detail { get; set; }
  public int Priority { get; set; }
  public TitleReferenceDTO? Title { get; set; }

  // Additional property to hold Individual's name from its DTOs

}

public class ContributorDTO
{
  public string Tconst { get; set; } = string.Empty!;
  public string Iconst { get; set; } = string.Empty!;
  public string Contribution { get; set; } = string.Empty!;
  public int Priority { get; set; }
}

public class ContributorReferenceDTO
{
  public string Tconst { get; set; } = string.Empty!;
  public string Iconst { get; set; } = string.Empty!;
}