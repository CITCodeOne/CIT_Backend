namespace DataService.DTOs;

// DTO for an episode row
public class EpisodeDTO
{
  public string Id { get; set; } = string.Empty; // episode tconst
  public string? ParentId { get; set; }          // series/parent tconst
  public int? Season { get; set; }
  public int? EpisodeNumber { get; set; }
}
