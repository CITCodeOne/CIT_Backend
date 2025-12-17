namespace BusinessLayer.Parameters;

public class IndividualSearchParameters
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Name { get; set; }
    public int? MinBirthYear { get; set; }
    public int? MaxBirthYear { get; set; }
    public string SortBy { get; set; } = "numvotes"; // name, birthyear, numvotes
    public bool SortDescending { get; set; } = false;
}
