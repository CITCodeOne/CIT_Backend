using AutoMapper;
using DataAccessLayer.Data;
using BusinessLayer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

public class RatingService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    // Constructor with dependency injection
    public RatingService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx; // Database context
        _mapper = mapper; // Mapping profile for DTOs
    }

    // Get rating by title ID and user ID (composite key lookup)
    public RatingDTO? GetRating(int uconst, string tconst)
    {
        var rating = _ctx.Ratings
            .FirstOrDefault(r => r.Uconst == uconst && r.Tconst == tconst);
        return rating == null ? null : _mapper.Map<RatingDTO>(rating);
    }

    // Get ratings by either user ID or title ID
    public List<RatingDTO> GetRatings(int? uconst = null, string? tconst = null)
    {
        if (!((uconst == null) != (tconst == null))) // XOR to ensure we only get one key
        {
            throw new ArgumentException("Either 'uconst' or 'tconst' must be provided, but not both.");
        }

        var ratings = _ctx.Ratings.Where(r =>
            (uconst != null && r.Uconst == uconst) ||
            (tconst != null && r.Tconst == tconst))
            .ToList();
        return _mapper.Map<List<RatingDTO>>(ratings);
    }
}

