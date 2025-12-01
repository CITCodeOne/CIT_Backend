using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using BusinessLayer.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/auth")]
public class AuthController : ControllerBase
{
    private readonly MdbService _mdbService;
    private readonly IConfiguration _configuration;

    public AuthController(MdbService mdbService, IConfiguration configuration)
    {
        _mdbService = mdbService;
        _configuration = configuration;
    }

    [HttpPost("signup")]
    public IActionResult SignUp(UserRegistrationDTO model)
    {
        try
        {
            var createdUser = _mdbService.Auth.Register(model);
            return Ok(new { message = "User created successfully", username = createdUser.Username });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public IActionResult Login(UserLoginDTO model)
    {
        try
        {
            var authUser = _mdbService.Auth.Authenticate(model);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, authUser.Username),
                new Claim(ClaimTypes.Role, authUser.Role),
                new Claim("uid", authUser.UserId.ToString())
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

            return Ok(new { username = authUser.Username, token = jwt });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException)
        {
            return BadRequest("Invalid username or password");
        }
    }
}
