using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using BusinessLayer.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebServiceLayer.Models;

namespace WebServiceLayer.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly MdbService _mdbService;
    private readonly Hashing _hashing;
    private readonly IConfiguration _configuration;

    public UserController(MdbService mdbService, Hashing hashing, IConfiguration configuration)
    {
        _mdbService = mdbService;
        _hashing = hashing;
        _configuration = configuration;
    }

    [HttpPost]
    public IActionResult SignUp(CreateUserModel model)
    {
        if (_mdbService.User.GetUser(model.Username) != null)
        {
            return BadRequest("User already exists");
        }

        if (string.IsNullOrEmpty(model.Password))
        {
            return BadRequest("Password is required");
        }

        if (string.IsNullOrEmpty(model.Email))
        {
            return BadRequest("Email is required");
        }

        (var hashedPwd, var salt) = _hashing.Hash(model.Password);

        _mdbService.User.CreateUser(model.Name, model.Username, model.Email, hashedPwd, salt, "User");
        // Default role is "User" whereas admin users should be created directly in the database or via a separate admin-only endpoint

        return Ok(new { message = "User created successfully" });
    }

    [HttpPost("login")]
    public IActionResult Login(UserLoginModel model)
    {
        var user = _mdbService.User.GetUser(model.Username);

        if (user == null)
        {
            return BadRequest("Invalid username or password");
        }

        if (!_hashing.Verify(model.Password, user.UPassword ?? "", user.Salt ?? ""))
        {
            return BadRequest("Invalid username or password");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("uid", user.Uconst.ToString())
        };

        var secret = _configuration.GetSection("Auth:Secret").Value
            ?? throw new InvalidOperationException("Auth:Secret is not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(4),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { username = user.UserName, token = jwt });
    }

    // NOTE: Arguably, we should have had both bookmarks and ratings under the URI path "api/user/{userId}/..." since they could be seen as resources owned by specific users. This would align with how RESTful APIs are intended to be structured.
}
