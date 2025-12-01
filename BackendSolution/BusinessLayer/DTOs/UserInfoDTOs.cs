namespace BusinessLayer.DTOs;

public class UserInfoFullDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    // public string? Password { get; set; } // Excluded as this is otherwise too easy to expose
    public string? Email { get; set; }
    public DateTime? Time { get; set; }
    //public ICollection<RatingDTO> RatingRefs { get; set; } = [];
    //public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    //public virtual ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();
    //public virtual ICollection<VisitedPage> VisitedPages { get; set; } = new List<VisitedPage>();
}

public class UserInfoReferenceDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}

public class CreateOrUpdateUserDTO
{
    public required string Name { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
}
