namespace BusinessLayer.DTOs;

public class PageFullDTO
{
    public int PageId { get; set; }

    //public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public IndividualReferenceDTO? Individual { get; set; }

    public TitleReferenceDTO? Title { get; set; }

    //public virtual ICollection<VisitedPage> VisitedPages { get; set; } = new List<VisitedPage>();
}

public class PageReferenceDTO
{
    public int PageId { get; set; }

    public string? Tconst { get; set; }

    public string? Iconst { get; set; }
}
