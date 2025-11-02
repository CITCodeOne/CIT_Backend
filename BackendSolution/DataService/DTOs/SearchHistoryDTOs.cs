namespace DataService.DTOs;

public class SearchHistoryDTO
{
    public int UserId { get; set; }
    public string SearchTerms { get; set; } = string.Empty;
    public DateTime Time { get; set; }
}