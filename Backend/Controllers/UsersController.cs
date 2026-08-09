using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzervizPont.Data;
using SzervizPont.Models;

namespace SzervizPont.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = Roles.Admin)]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .OrderBy(user => user.Name)
            .Select(user => new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role
            })
            .ToListAsync();

        return Ok(users);
    }
}
