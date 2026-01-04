using AutoMapper;
using DataAccessLayer.Data;
using DataAccessLayer.Entities;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services;

// Service til håndtering af bookmarks (favoritter) for brugere.
// Forklaring for ikke-teknikere: et bookmark knytter en bruger til en side,
// så brugeren senere kan finde den side hurtigt.
public class BookmarkService
{
    private readonly CITContext _ctx; // Database-kontekst
    private readonly IMapper _mapper; // Mapper til at konvertere mellem entiteter og DTO'er

    public BookmarkService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    // Hent et specifikt bookmark for en bruger og en side.
    // Returnerer null hvis bookmark ikke findes.
    public BookmarkDTO? GetBookmark(int uconst, int pconst)
    {
        var bookmark = _ctx.Bookmarks.Where(b => b.Uconst == uconst && b.Pconst == pconst).FirstOrDefault();
        if (bookmark == null) return null;
        return _mapper.Map<BookmarkDTO>(bookmark);
    }

    // Hent alle bookmarks for en bruger.
    public List<BookmarkDTO> GetBookmarksByUser(int uconst)
    {
        var bookmarks = _ctx.Bookmarks.Where(b => b.Uconst == uconst).ToList();
        return _mapper.Map<List<BookmarkDTO>>(bookmarks);
    }

    // Tilføj et nyt bookmark for en bruger til en side.
    // Hvis siden ikke findes kastes en undtagelse, hvis bookmark allerede findes returnerer vi null.
    public BookmarkDTO? AddBookmark(int uconst, int pconst)
    {
        var page = _ctx.Pages.Find(pconst);
        if (page == null)
            throw new InvalidOperationException($"Page with id {pconst} does not exist.");

        var existing = _ctx.Bookmarks.Find(uconst, pconst);
        if (existing != null)
            return null; // Bookmark eksisterer allerede

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

    // Fjern et bookmark for en bruger og side. Returnerer true hvis fjernet, false hvis ikke fundet.
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
