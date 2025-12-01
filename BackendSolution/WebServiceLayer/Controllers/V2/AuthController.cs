using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using BusinessLayer.Services;
using WebServiceLayer.Models;

namespace WebServiceLayer.Controllers.V2;

[ApiController]
[Route("api/v2/auth")]
public class AuthController : ControllerBase
{
    private readonly MdbService _mdbService;
    private readonly IConfiguration _configuration;

    public AuthController(MdbService mdbService, Hashing hashing, IConfiguration configuration)
    {
        _mdbService = mdbService;
        _configuration = configuration;
    }

    [HttpPost("signup")]
    public IActionResult SignUp(CreateUserModel model)
    {
        return Ok(new { message = "SignUp endpoint placeholder" });
    }

    [HttpPost("login")]
    public IActionResult Login(UserLoginModel model)
    {
        return Ok(new { message = "Login endpoint placeholder" });
    }
}
