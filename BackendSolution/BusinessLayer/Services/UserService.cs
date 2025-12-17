using DataAccessLayer.Data;
using BusinessLayer.DTOs;
using AutoMapper;

namespace BusinessLayer.Services;

public class UserService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    public UserService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    public UserInfoFullDTO? GetUserById(int uconst)
    {
        var user = _ctx.UserInfos.FirstOrDefault(u => u.Uconst == uconst);
        return user == null ? null : _mapper.Map<UserInfoFullDTO>(user);
    }

    public string? GetProfileImage(int userId)
    {
        return _ctx.UserInfos
            .Where(u => u.Uconst == userId)
            .Select(u => u.ProfileImage)
            .SingleOrDefault();
    }

    public bool SetProfileImage(int userId, string base64Image)
    {
        var user = _ctx.UserInfos.SingleOrDefault(u => u.Uconst == userId);
        if (user == null) return false;

        user.ProfileImage = base64Image;
        _ctx.SaveChanges();
        return true;
    }
}
