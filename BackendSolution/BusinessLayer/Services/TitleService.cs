using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

public class TitleService : ITitleService
{
    private const int MaxPageSize = 100;
    private readonly CITContext _db;
    private readonly IMapper _mapper;

    public TitleService(CITContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<TitleFullDTO?> GetByIdAsync(string tconst, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tconst)) return null;

        // Project to DTO at the database to avoid over-fetching
        var dto = await _db.Titles
            .AsNoTracking()
            .Where(t => t.Tconst == tconst)
            .ProjectTo<TitleFullDTO>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(ct);

        return dto;
    }

    public async Task<IReadOnlyList<TitlePreviewDTO>> GetByGenreAsync(
        int genreId,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var skip = (page - 1) * pageSize;

        var query = _db.Titles
            .AsNoTracking()
            .Where(t => t.Gconsts.Any(g => g.Gconst == genreId))
            .OrderByDescending(t => t.AvgRating) // simple default sort; can expand later
            .ThenByDescending(t => t.Numvotes)
            .ProjectTo<TitlePreviewDTO>(_mapper.ConfigurationProvider)
            .Skip(skip)
            .Take(pageSize);

        var list = await query.ToListAsync(ct);
        return list;
    }
}
