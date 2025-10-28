using DataService.Data;
using Microsoft.EntityFrameworkCore;

namespace DataService.Entities;

public class GenreService
{
    private readonly CITContext _context;

    public GenreService(CITContext context)
    {
        _context = context;
    }
    public DbSet<Genre> GetGenreSet()
    {
        return _context.Genres;
    }
    public CITContext GetFullContext()
    {
        return _context;
    }
}
