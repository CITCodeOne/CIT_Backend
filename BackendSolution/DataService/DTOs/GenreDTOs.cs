namespace DataService.DTOs;

public class GenreDTO
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
}

public class GenreFullDTO
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public List<TitleReferenceDTO> Titles { get; set; } = new();
}
