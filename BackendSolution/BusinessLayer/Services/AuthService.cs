using AutoMapper;
using DataAccessLayer.Data;
using BusinessLayer.DTOs;

namespace BusinessLayer.Services;

public class AuthService
{
    private readonly CITContext _ctx;
    private readonly IMapper _mapper;

    public AuthService(CITContext ctx, IMapper mapper)
    {
        _ctx = ctx;
        _mapper = mapper;
    }

    public bool ValidateUserCredentials(string username, string password)
    {
        var user = _ctx.UserInfos.FirstOrDefault(u => u.UserName == username && u.UPassword == password);
        return user != null;
    }
}
