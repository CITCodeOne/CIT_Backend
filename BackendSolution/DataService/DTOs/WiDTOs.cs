namespace DataService.DTOs;

// Minimal DTO for word index rows
public class WiDTO
{
  public string TitleId { get; set; } = string.Empty; // tconst
  public string Word { get; set; } = string.Empty;
  public string Field { get; set; } = string.Empty;   // single-char field as string
  public string? Lexeme { get; set; }
}
