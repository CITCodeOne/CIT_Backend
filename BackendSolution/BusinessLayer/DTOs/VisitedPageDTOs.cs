namespace BusinessLayer.DTOs;

// Used to return a visited page with basic info
public class VisitedPageDTO
{
    public int UserId { get; set; }
    public int PageId { get; set; }
    public DateTime Time { get; set; }
}

// Used to return a visited page with page details (title or individual)
public class VisitedPageWithDetailsDTO
{
    public int UserId { get; set; }
    public int PageId { get; set; }
    public DateTime Time { get; set; }
    public string? TitleId { get; set; }
    public string? IndividualId { get; set; }
}

// Used to create a new visited page entry
public class CreateVisitedPageDTO
{
    public int PageId { get; set; }
}
