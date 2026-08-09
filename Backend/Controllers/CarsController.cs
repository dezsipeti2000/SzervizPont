using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzervizPont.Data;
using SzervizPont.DTOs;
using SzervizPont.Models;
using System.Security.Claims;

namespace SzervizPont.Controllers;

[ApiController]
[Route("api/cars")]
[Authorize]
public class CarsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CarsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool all = false)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole(Roles.Admin);

        var query = _db.Cars
            .Include(car => car.User)
            .AsQueryable();

        if (!isAdmin || !all)
            query = query.Where(car => car.UserId == userId);

        var cars = await query
            .OrderBy(car => car.PlateNumber)
            .Select(car => new
            {
                car.Id,
                car.UserId,
                UserName = car.User != null ? car.User.Name : "",
                car.PlateNumber,
                car.Brand,
                car.Model,
                car.Year
            })
            .ToListAsync();

        return Ok(cars);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var car = await _db.Cars
            .Include(item => item.User)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (car is null)
            return NotFound(new { message = "Az autó nem található." });

        if (!CanManage(car))
            return Forbid();

        return Ok(new
        {
            car.Id,
            car.UserId,
            UserName = car.User != null ? car.User.Name : "",
            car.PlateNumber,
            car.Brand,
            car.Model,
            car.Year
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CarRequest request)
    {
        var validationMessage = ValidateCar(request);
        if (validationMessage != null)
            return BadRequest(new { message = validationMessage });

        var plateNumber = NormalizePlate(request.PlateNumber);
        if (await _db.Cars.AnyAsync(car => car.PlateNumber == plateNumber))
            return BadRequest(new { message = "Ez a rendszám már szerepel a rendszerben." });

        var car = new Car
        {
            UserId = GetCurrentUserId(),
            PlateNumber = plateNumber,
            Brand = request.Brand.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year
        };

        _db.Cars.Add(car);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Az autó mentve lett." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CarRequest request)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(item => item.Id == id);
        if (car is null)
            return NotFound(new { message = "Az autó nem található." });

        if (!CanManage(car))
            return Forbid();

        var validationMessage = ValidateCar(request);
        if (validationMessage != null)
            return BadRequest(new { message = validationMessage });

        var plateNumber = NormalizePlate(request.PlateNumber);
        if (await _db.Cars.AnyAsync(item => item.Id != id && item.PlateNumber == plateNumber))
            return BadRequest(new { message = "Ez a rendszám már szerepel a rendszerben." });

        car.PlateNumber = plateNumber;
        car.Brand = request.Brand.Trim();
        car.Model = request.Model.Trim();
        car.Year = request.Year;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Az autó módosítva lett." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(item => item.Id == id);
        if (car is null)
            return NotFound(new { message = "Az autó nem található." });

        if (!CanManage(car))
            return Forbid();

        if (await _db.Appointments.AnyAsync(appointment => appointment.CarId == id))
            return BadRequest(new { message = "Az autó nem törölhető, mert tartozik hozzá időpont." });

        _db.Cars.Remove(car);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Az autó törölve lett." });
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }

    private bool CanManage(Car car)
    {
        return User.IsInRole(Roles.Admin) || car.UserId == GetCurrentUserId();
    }

    private static string NormalizePlate(string plateNumber)
    {
        return plateNumber.Trim().ToUpper().Replace(" ", "").Replace("-", "");
    }

    private static string? ValidateCar(CarRequest request)
    {
        var plateNumber = NormalizePlate(request.PlateNumber);

        if (plateNumber.Length < 5 || plateNumber.Length > 20)
            return "A rendszám 5 és 20 karakter közötti legyen.";

        if (!plateNumber.All(char.IsLetterOrDigit))
            return "A rendszám csak betűket és számokat tartalmazhat.";

        if (request.Brand.Trim().Length < 2)
            return "A márka megadása kötelező.";

        if (request.Model.Trim().Length < 1)
            return "A modell megadása kötelező.";

        if (request.Year < 1980 || request.Year > DateTime.Now.Year + 1)
            return "Nem megfelelő évjárat.";

        return null;
    }
}
