using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzervizPont.Data;
using SzervizPont.DTOs;
using SzervizPont.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SzervizPont.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthController(AppDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var name = request.Name.Trim();
        var email = request.Email.Trim().ToLower();
        var password = request.Password;

        if (name.Length < 3 || name.Length > 100)
            return BadRequest(new { message = "A név 3 és 100 karakter közötti legyen." });

        if (!new EmailAddressAttribute().IsValid(email))
            return BadRequest(new { message = "Érvénytelen email cím." });

        if (password.Length < 8)
            return BadRequest(new { message = "A jelszó legalább 8 karakter hosszú legyen." });

        if (await _db.Users.AnyAsync(user => user.Email == email))
            return BadRequest(new { message = "Ez az email cím már használatban van." });

        var user = new User
        {
            Name = name,
            Email = email,
            Role = Roles.User
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await SignInUser(user);
        return Ok(new { user = CreateUserData(user) });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _db.Users.FirstOrDefaultAsync(item => item.Email == email);

        if (user is null)
            return Unauthorized(new { message = "Hibás email cím vagy jelszó." });

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Hibás email cím vagy jelszó." });

        await SignInUser(user);
        return Ok(new { user = CreateUserData(user) });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0"),
            name = User.FindFirstValue(ClaimTypes.Name) ?? "",
            email = User.FindFirstValue(ClaimTypes.Email) ?? "",
            role = User.FindFirstValue(ClaimTypes.Role) ?? Roles.User
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Sikeres kijelentkezés." });
    }

    private async Task SignInUser(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);
    }

    private static object CreateUserData(User user)
    {
        return new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Role
        };
    }
}
