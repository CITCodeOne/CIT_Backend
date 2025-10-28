namespace DataService.DTOs;

public class TitleFullDTO
{
  public string Id { get; set; } = string.Empty;
  public string? Name { get; set; }
  public string MediaType { get; set; } = string.Empty;
  public double AvgRating { get; set; }
  public int NumVotes { get; set; }
  public DateTime ReleaseDate { get; set; }
  public bool Adult { get; set; }
  public int StartYear { get; set; }
  public int? EndYear { get; set; }
  public int Runtime { get; set; }
  public string? Poster { get; set; }
  public string? PlotPre { get; set; }
  public List<GenreDTO>? Genres { get; set; }
}

public class TitlePreviewDTO
{
  public string Id { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public string MediaType { get; set; } = string.Empty;
  public double AvgRating { get; set; }
  public DateTime ReleaseDate { get; set; }
  public string Poster { get; set; } = string.Empty;
}

public class TitleReferenceDTO
{
  public string Id { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
}