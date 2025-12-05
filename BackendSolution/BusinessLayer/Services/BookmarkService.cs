using AutoMapper;
using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services;

public class BookmarkService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    public BookmarkService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    public BookmarkDTO? GetBookmark(int uconst, int pconst)
    {
        var bookmark = _ctx.Bookmarks.Where(b => b.Uconst == uconst && b.Pconst == pconst).FirstOrDefault();
        if (bookmark == null) return null;
        return _mapper.Map<BookmarkDTO>(bookmark);
    }

    public List<BookmarkDTO> GetBookmarksByUser(int uconst)
    {
        var bookmarks = _ctx.Bookmarks.Where(b => b.Uconst == uconst).ToList();
        return _mapper.Map<List<BookmarkDTO>>(bookmarks);
    }

    public BookmarkDTO? AddBookmark(int uconst, int pconst)
    {
        var page = _ctx.Pages.Find(pconst);
        if (page == null)
            throw new InvalidOperationException($"Page with id {pconst} does not exist.");

        var existing = _ctx.Bookmarks.Find(uconst, pconst);
        if (existing != null)
            return null;

        var bm = new Bookmark
        {
            Uconst = uconst,
            Pconst = pconst,
            Time = DateTime.UtcNow
        };

        _ctx.Bookmarks.Add(bm);
        _ctx.SaveChanges();
        return _mapper.Map<BookmarkDTO>(bm);
    }

    public bool RemoveBookmark(int uconst, int pconst)
    {
        var bookmark = _ctx.Bookmarks.Where(b => b.Uconst == uconst && b.Pconst == pconst).FirstOrDefault();
        if (bookmark == null)
            return false;

        _ctx.Bookmarks.Remove(bookmark);
        _ctx.SaveChanges();

        return true;
    }
}
