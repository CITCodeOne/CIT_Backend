using System;
using System.Collections.Generic;

namespace DataAccessLayer.Entities;

public partial class Page
{
    public int Pconst { get; set; }

    public string? Iconst { get; set; }

    public string? Tconst { get; set; }

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual Individual? IconstNavigation { get; set; }

    public virtual Title? TconstNavigation { get; set; }

    public virtual ICollection<VisitedPage> VisitedPages { get; set; } = new List<VisitedPage>();
}
