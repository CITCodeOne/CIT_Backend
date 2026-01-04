using DataAccessLayer.Data;
using BusinessLayer.DTOs;
using AutoMapper;

namespace BusinessLayer.Services;

// Service til at hente og opdatere brugeroplysninger.
// Forklaring: denne klasse bruges til at læse brugerprofiler, hente og opdatere profilbilleder.
public class UserService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    public UserService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    // Hent fuld brugerinformation ud fra internt bruger-id (uconst).
    // Returnerer null hvis brugeren ikke findes.
    public UserInfoFullDTO? GetUserById(int uconst)
    {
        var user = _ctx.UserInfos.FirstOrDefault(u => u.Uconst == uconst);
        return user == null ? null : _mapper.Map<UserInfoFullDTO>(user);
    }

    // Hent kun profilbilledet (som base64-streng) for en bruger.
    // Hvis brugeren ikke har et profilbillede returneres null.
    public string? GetProfileImage(int userId)
    {
        return _ctx.UserInfos
            .Where(u => u.Uconst == userId)
            .Select(u => u.ProfileImage)
            .SingleOrDefault();
    }

    // Sæt eller opdater profilbillede for en bruger.
    // Input forventes at være et base64-kodet billede (tekstformat).
    public bool SetProfileImage(int userId, string base64Image)
    {
        var user = _ctx.UserInfos.SingleOrDefault(u => u.Uconst == userId);
        if (user == null) return false;

        user.ProfileImage = base64Image;
        _ctx.SaveChanges();
        return true;
    }
}
