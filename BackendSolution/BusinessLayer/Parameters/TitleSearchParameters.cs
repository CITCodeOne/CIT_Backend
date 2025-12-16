namespace BusinessLayer.Parameters;

public class TitleSearchParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Genre { get; set; }
    public string? MediaType { get; set; }
    public string? TitleSearchTerm { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public double? MinRating { get; set; }
    public bool? IsAdult { get; set; }
    public string SortBy { get; set; } = "rating"; // rating, year, title
    public bool SortDescending { get; set; } = true;
}
