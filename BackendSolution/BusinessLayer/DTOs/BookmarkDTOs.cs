namespace BusinessLayer.DTOs;

// Used to return a bookmark
public class BookmarkDTO
{
    public int UserId { get; set; }
    public int PageId { get; set; }
    public DateTime? Time { get; set; }
}

// Used to return a bookmark with page details
public class BookmarkWithDetailsDTO
{
    public int UserId { get; set; }
    public int PageId { get; set; }
    public DateTime? Time { get; set; }
    public string? TitleId { get; set; }
    public string? IndividualId { get; set; }
}

// Used to create a new bookmark
public class CreateBookmarkDTO
{
    public int PageId { get; set; }
}
