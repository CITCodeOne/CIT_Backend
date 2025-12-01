using Microsoft.AspNetCore.Mvc;
using BusinessLayer;
using BusinessLayer.Services;

namespace WebServiceLayer.Controllers.V1;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly MdbService _mdbService;
    private readonly IConfiguration _configuration;

    public UserController(MdbService mdbService, Hashing hashing, IConfiguration configuration)
    {
        _mdbService = mdbService;
        _configuration = configuration;
    }

    // This controller is depricated as it has been replaced by UsersController in V2 while this one originally handled a lot of authentication which is moved to AuthController.
}
