using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SzervizPont.Data;
using SzervizPont.DTOs;
using SzervizPont.Models;
using System.Security.Claims;

namespace SzervizPont.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AppointmentsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool all = false)
    {
        var userId = GetCurrentUserId();
        var isAdmin = User.IsInRole(Roles.Admin);

        var query = _db.Appointments
            .Include(appointment => appointment.User)
            .Include(appointment => appointment.Car)
            .Include(appointment => appointment.ServiceItem)
            .AsQueryable();

        if (!isAdmin || !all)
            query = query.Where(appointment => appointment.UserId == userId);

        var appointments = await query
            .OrderByDescending(appointment => appointment.StartAt)
            .Select(appointment => new
            {
                appointment.Id,
                appointment.UserId,
                UserName = appointment.User != null ? appointment.User.Name : "",
                appointment.CarId,
                PlateNumber = appointment.Car != null ? appointment.Car.PlateNumber : "",
                appointment.ServiceItemId,
                ServiceName = appointment.ServiceItem != null ? appointment.ServiceItem.Name : "",
                EstimatedMinutes = appointment.ServiceItem != null
                    ? appointment.ServiceItem.EstimatedMinutes
                    : 0,
                appointment.StartAt,
                appointment.Status,
                Note = appointment.Note ?? ""
            })
            .ToListAsync();

        return Ok(appointments);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AppointmentCreateRequest request)
    {
        if (request.CarId <= 0 || request.ServiceItemId <= 0)
            return BadRequest(new { message = "Az autó és a szolgáltatás kiválasztása kötelező." });

        if (request.StartAt < DateTime.Now.AddMinutes(30))
            return BadRequest(new { message = "Legalább 30 perccel későbbi időpontot válassz." });

        if (request.Note.Trim().Length > 500)
            return BadRequest(new { message = "A megjegyzés legfeljebb 500 karakter lehet." });

        var userId = GetCurrentUserId();
        var car = await _db.Cars.FirstOrDefaultAsync(item => item.Id == request.CarId);

        if (car is null || car.UserId != userId)
            return BadRequest(new { message = "A kiválasztott autó nem tartozik hozzád." });

        if (!await _db.Services.AnyAsync(item => item.Id == request.ServiceItemId))
            return BadRequest(new { message = "A szolgáltatás nem található." });

        if (await HasSameStartTime(request.StartAt))
            return BadRequest(new { message = "Erre a kezdési időpontra már van foglalás." });

        var appointment = new Appointment
        {
            UserId = userId,
            CarId = request.CarId,
            ServiceItemId = request.ServiceItemId,
            StartAt = request.StartAt,
            Status = AppointmentStatuses.Pending,
            Note = request.Note.Trim()
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Az időpont rögzítve lett." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, AppointmentUpdateRequest request)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(item => item.Id == id);
        if (appointment is null)
            return NotFound(new { message = "Az időpont nem található." });

        if (!CanManage(appointment))
            return Forbid();

        if (appointment.Status == AppointmentStatuses.Completed ||
            appointment.Status == AppointmentStatuses.Cancelled)
        {
            if (!User.IsInRole(Roles.Admin))
                return BadRequest(new { message = "Lezárt időpontot az ügyfél már nem módosíthat." });
        }

        if (request.StartAt < DateTime.Now.AddMinutes(30))
            return BadRequest(new { message = "Legalább 30 perccel későbbi időpontot válassz." });

        if (request.Note.Trim().Length > 500)
            return BadRequest(new { message = "A megjegyzés legfeljebb 500 karakter lehet." });

        if (await HasSameStartTime(request.StartAt, id))
            return BadRequest(new { message = "Erre a kezdési időpontra már van foglalás." });

        appointment.StartAt = request.StartAt;
        appointment.Note = request.Note.Trim();

        await _db.SaveChangesAsync();
        return Ok(new { message = "Az időpont módosítva lett." });
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateStatus(int id, AppointmentStatusUpdateRequest request)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(item => item.Id == id);
        if (appointment is null)
            return NotFound(new { message = "Az időpont nem található." });

        var status = request.Status.Trim();
        if (!AppointmentStatuses.All.Contains(status))
            return BadRequest(new { message = "Nem megfelelő státusz." });

        appointment.Status = status;
        await _db.SaveChangesAsync();
        return Ok(new { message = "A státusz módosítva lett." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(item => item.Id == id);
        if (appointment is null)
            return NotFound(new { message = "Az időpont nem található." });

        if (!CanManage(appointment))
            return Forbid();

        _db.Appointments.Remove(appointment);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Az időpont törölve lett." });
    }

    private async Task<bool> HasSameStartTime(DateTime startAt, int? ignoredId = null)
    {
        return await _db.Appointments.AnyAsync(appointment =>
            appointment.Status != AppointmentStatuses.Cancelled &&
            appointment.StartAt == startAt &&
            (!ignoredId.HasValue || appointment.Id != ignoredId.Value));
    }

    private int GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }

    private bool CanManage(Appointment appointment)
    {
        return User.IsInRole(Roles.Admin) || appointment.UserId == GetCurrentUserId();
    }
}
