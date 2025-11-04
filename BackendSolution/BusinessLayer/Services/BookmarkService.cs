using DataAccessLayer.Data;
using DataAccessLayer.Entities;

namespace BusinessLayer.Services;

public class BookmarkService
{
    private readonly CITContext _ctx;

    public BookmarkService(CITContext ctx)
    {
        _ctx = ctx;
    }

    public bool AddBookmark(int uconst, int pconst)
    {
        var page = _ctx.Pages.Find(pconst);
        if (page == null)
            throw new InvalidOperationException($"Page with id {pconst} does not exist.");

        var existing = _ctx.Bookmarks.Find(uconst, pconst);
        if (existing != null)
            return false; 

        var bm = new Bookmark
        {
            Uconst = uconst,
            Pconst = pconst,
            Time = DateTime.UtcNow
        };

        _ctx.Bookmarks.Add(bm);
        _ctx.SaveChanges();

        return true;
    }

    public IEnumerable<Bookmark> GetBookmarksForUser(int uconst)
    {
        return _ctx.Bookmarks.Where(b => b.Uconst == uconst).ToList();
    }
}
