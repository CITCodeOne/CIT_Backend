using BusinessLayer.DTOs;
using DataAccessLayer.Data;
using DataAccessLayer.Entities;

namespace BusinessLayer.Services;

public class AuthService
{
    private readonly CITContext _ctx;
    private readonly Hashing _hashing;

    public AuthService(CITContext ctx, Hashing hashing)
    {
        _ctx = ctx;
        _hashing = hashing;
    }

    public AuthUserDTO Register(UserRegistrationDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var username = DetermineUsername(dto);
        ValidateRegistration(username, dto.Email, dto.Password);

        if (_ctx.UserInfos.Any(u => u.UserName == username))
        {
            throw new InvalidOperationException("User already exists");
        }

        var (hashedPwd, salt) = _hashing.Hash(dto.Password);

        var user = new UserInfo
        {
            UserName = username,
            UPassword = hashedPwd,
            Email = dto.Email?.Trim(),
            Salt = salt,
            Role = "User",
            Time = DateTime.UtcNow
        };

        _ctx.UserInfos.Add(user);
        _ctx.SaveChanges();

        return BuildAuthUser(user);
    }

    public AuthUserDTO Authenticate(UserLoginDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Username and password are required");
        }

        var user = _ctx.UserInfos.FirstOrDefault(u => u.UserName == dto.Username.Trim());
        if (user == null || !_hashing.Verify(dto.Password, user.UPassword ?? string.Empty, user.Salt ?? string.Empty))
        {
            throw new InvalidOperationException("Invalid username or password");
        }

        return BuildAuthUser(user);
    }

    private static string DetermineUsername(UserRegistrationDTO dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Username))
        {
            return dto.Username.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            return dto.Name.Trim();
        }

        return string.Empty;
    }

    private static void ValidateRegistration(string username, string? email, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required", nameof(password));
        }
    }

    private static AuthUserDTO BuildAuthUser(UserInfo user) => new()
    {
        UserId = user.Uconst,
        Username = user.UserName,
        Role = user.Role
    };
}
