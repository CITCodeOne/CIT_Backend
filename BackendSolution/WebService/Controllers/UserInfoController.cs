using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using DataService.Entities;
using DataService.DTOs;
using DataService.Data;

namespace WebService.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly CITContext _context;
    private readonly IMapper _mapper;

    public UserController(CITContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    //Get all user references
    [HttpGet("ref")]
    public async Task<ActionResult<List<UserInfoReferenceDTO>>> GetAllUserReferences()
    {
        int pageSize = 100;

        var users = await _context.UserInfos
                            .OrderBy(u => u.UserName)
                            .Take(pageSize)
                            .ToListAsync();
        return Ok(_mapper.Map<List<UserInfoReferenceDTO>>(users));
    }
    //Get all users
    [HttpGet]
    public async Task<ActionResult<List<UserInfoFullDTO>>> GetAllUsers()
    {
        int pageSize = 100;

        var users = await _context.UserInfos
                            .Include(u => u.Ratings)
                            .OrderBy(u => u.UserName)
                            .Take(pageSize)
                            .ToListAsync();
        return Ok(_mapper.Map<List<UserInfoFullDTO>>(users));
    }
    //Get user ref from uconst
    [HttpGet("{uconst}/ref")]
    public async Task<ActionResult<UserInfoReferenceDTO>> GetUserRefFromUconst(int uconst)
    {
        var user = await _context.UserInfos
                            .Where(u => u.Uconst == uconst)
                            .FirstOrDefaultAsync();
        if (user == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<UserInfoReferenceDTO>(user));
    }
    //Get user from uconst
    [HttpGet("{uconst}")]
    public async Task<ActionResult<UserInfoFullDTO>> GetUserFromUconst(int uconst)
    {
        var user = await _context.UserInfos
                            .Where(u => u.Uconst == uconst)
                            .FirstOrDefaultAsync();
        if (user == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<UserInfoFullDTO>(user));
    }

    //Create new user
    // POST: api/user
    [HttpPost]
    public async Task<ActionResult<UserInfoFullDTO>> CreateUser(CreateOrUpdateUserDTO createDto)
    {
        if (createDto == null)
        {
            return BadRequest("DTO is NULL");
        }

        //Skal ændres til at tjekke både null og empty, men er ikke sikker på syntaxen
        if (createDto.Name == null ||
            createDto.Password == null ||
            createDto.Email == null)
        {
            //Ved ikke om det er her der skal tjekkes for valid Email og password
            return BadRequest("Name, password or Email is NULL");
        }
        try
        {
            var nameCheck = await _context.UserInfos
                                            .Where(u => u.UserName == createDto.Name)
                                            .FirstOrDefaultAsync();
            if (nameCheck != null)
            {
                return BadRequest("Username already exists");
            }

            var mailCheck = await _context.UserInfos
                                            .Where(u => u.Email == createDto.Email)
                                            .FirstOrDefaultAsync();
            if (mailCheck != null)
            {
                return BadRequest("Email already exists");
            }

            await _context.Database.ExecuteSqlRawAsync(
              "SELECT mdb.create_user({0}, {1}, {2})",
              createDto.Name, //{0}
              createDto.Password, //{1}
              createDto.Email //{2}
            );
            var newUser = await _context.UserInfos
              .FirstOrDefaultAsync(u => u.UserName == createDto.Name);

            if (newUser == null)
            {
                return StatusCode(500, "User was created but could not be retrieved");
            }
            return Ok(_mapper.Map<UserInfoFullDTO>(newUser));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    //Edit userinfo (Funktion skal tilføjes i databasen)
    /*[HttpPost("{uconst}")]
    public async Task<ActionResult<UserInfoFullDTO>> UpdateUserInfo(CreateOrUpdateUserDTO updateDto, int uconst)
    {
        if (updateDto == null)
        {
            return BadRequest("DTO is NULL");
        }

    }
    */
    //Delete User (Funktion skal tilføjes i databasen)
}
