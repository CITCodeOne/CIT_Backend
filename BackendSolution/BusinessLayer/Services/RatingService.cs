using AutoMapper;
using DataAccessLayer.Data;
using BusinessLayer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Services;

// Service til at håndtere bedømmelser (ratings) som brugere sætter på titler.
// Brugere kan give en score (1-10) og evt. en tekstkommentar.
// Denne service henter, opretter/ændrer og sletter ratings ved at interagere med databasen.
public class RatingService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    // Konstruktør: modtager database-kontekst og mapper
    public RatingService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx; // Database context
        _mapper = mapper; // Mapping profile for DTOs
    }

    // Hent en specifik rating for en bruger og titel.
    // Hvis brugeren ikke har rated titlen returneres null.
    public RatingDTO? GetRating(int uconst, string tconst)
    {
        var rating = _ctx.Ratings
            .FirstOrDefault(r => r.Uconst == uconst && r.Tconst == tconst);
        return rating == null ? null : _mapper.Map<RatingDTO>(rating);
    }

    // Hent ratings enten for en bruger (uconst) eller for en titel (tconst).
    // Bemærk: enten-eller er påkrævet (ikke begge samtidig).
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

    // Opret eller opdater en rating asynkront.
    // Vi sender opgaven videre til en database-funktion (`mdb.rate`) som håndterer logikken.
    public async Task RateAsync(int uconst, string tconst, int rating, string? reviewText = null)
    {
        if (string.IsNullOrWhiteSpace(tconst))
        {
            throw new ArgumentException("Title id is required", nameof(tconst));
        }

        if (rating < 1 || rating > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 10");
        }

        // Kald database-funktionen der opretter eller opdaterer rating og opdaterer aggregerede værdier
        await _ctx.Database.ExecuteSqlInterpolatedAsync($"SELECT mdb.rate({uconst}, {tconst}, {rating}, {reviewText});");
    }

    // Slet en rating asynkront ved at kalde en database-funktion
    public async Task DeleteRatingAsync(int uconst, string tconst)
    {
        if (string.IsNullOrWhiteSpace(tconst))
        {
            throw new ArgumentException("Title id is required", nameof(tconst));
        }

        await _ctx.Database.ExecuteSqlInterpolatedAsync($"SELECT mdb.delete_rating({uconst}, {tconst});");
    }
}

