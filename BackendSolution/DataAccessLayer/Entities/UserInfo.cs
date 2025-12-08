using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class UserInfo
{
    public int Uconst { get; set; }

    public string UserName { get; set; } = null!;

    public string? UPassword { get; set; }

    public string? Email { get; set; }

    public DateTime? Time { get; set; }

    // Authentication fields
    public string? Salt { get; set; }
    
    public string Role { get; set; } = "User";

    public string? ProfileImage { get; set; }

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<SearchHistory> SearchHistories { get; set; } = new List<SearchHistory>();

    public virtual ICollection<VisitedPage> VisitedPages { get; set; } = new List<VisitedPage>();
}
