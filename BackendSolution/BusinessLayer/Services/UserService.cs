using DataAccessLayer.Data;
using DataAccessLayer.Entities;
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

    public UserInfo? GetUser(string username)
    {
        return _ctx.UserInfos.FirstOrDefault(u => u.UserName == username);
    }


    public UserInfo CreateUser(string name, string username, string hashedPassword, string salt, string role)
    {
        var user = new UserInfo
        {
            UserName = username,
            UPassword = hashedPassword,
            Salt = salt,
            Role = role,
            Email = name,
            Time = DateTime.UtcNow
        };

        _ctx.UserInfos.Add(user);
        _ctx.SaveChanges();

        return user;
    }
}
