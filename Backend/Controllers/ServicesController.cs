using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzervizPont.Data;
using SzervizPont.DTOs;
using SzervizPont.Models;

namespace SzervizPont.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ServicesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var services = await _db.Services
            .OrderBy(service => service.Name)
            .Select(service => new
            {
                service.Id,
                service.Name,
                service.Description,
                service.Price,
                service.EstimatedMinutes
            })
            .ToListAsync();

        return Ok(services);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await _db.Services.FirstOrDefaultAsync(item => item.Id == id);
        if (service is null)
            return NotFound(new { message = "A szolgáltatás nem található." });

        return Ok(new
        {
            service.Id,
            service.Name,
            service.Description,
            service.Price,
            service.EstimatedMinutes
        });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(ServiceRequest request)
    {
        var validationMessage = ValidateService(request);
        if (validationMessage != null)
            return BadRequest(new { message = validationMessage });

        var name = request.Name.Trim();
        if (await _db.Services.AnyAsync(item => item.Name.ToLower() == name.ToLower()))
            return BadRequest(new { message = "Már létezik ilyen nevű szolgáltatás." });

        var service = new ServiceItem
        {
            Name = name,
            Description = request.Description.Trim(),
            Price = request.Price,
            EstimatedMinutes = request.EstimatedMinutes
        };

        _db.Services.Add(service);
        await _db.SaveChangesAsync();
        return Ok(new { message = "A szolgáltatás mentve lett." });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(int id, ServiceRequest request)
    {
        var service = await _db.Services.FirstOrDefaultAsync(item => item.Id == id);
        if (service is null)
            return NotFound(new { message = "A szolgáltatás nem található." });

        var validationMessage = ValidateService(request);
        if (validationMessage != null)
            return BadRequest(new { message = validationMessage });

        var name = request.Name.Trim();
        if (await _db.Services.AnyAsync(item => item.Id != id && item.Name.ToLower() == name.ToLower()))
            return BadRequest(new { message = "Már létezik ilyen nevű szolgáltatás." });

        service.Name = name;
        service.Description = request.Description.Trim();
        service.Price = request.Price;
        service.EstimatedMinutes = request.EstimatedMinutes;

        await _db.SaveChangesAsync();
        return Ok(new { message = "A szolgáltatás módosítva lett." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await _db.Services.FirstOrDefaultAsync(item => item.Id == id);
        if (service is null)
            return NotFound(new { message = "A szolgáltatás nem található." });

        if (await _db.Appointments.AnyAsync(appointment => appointment.ServiceItemId == id))
            return BadRequest(new { message = "A szolgáltatás nem törölhető, mert tartozik hozzá időpont." });

        _db.Services.Remove(service);
        await _db.SaveChangesAsync();
        return Ok(new { message = "A szolgáltatás törölve lett." });
    }

    private static string? ValidateService(ServiceRequest request)
    {
        if (request.Name.Trim().Length < 3)
            return "A szolgáltatás neve legalább 3 karakter legyen.";

        if (request.Description.Trim().Length > 500)
            return "A leírás legfeljebb 500 karakter lehet.";

        if (request.Price < 0)
            return "Az ár nem lehet negatív.";

        if (request.EstimatedMinutes < 10 || request.EstimatedMinutes > 1440)
            return "Az időtartam 10 és 1440 perc közötti lehet.";

        return null;
    }
}
