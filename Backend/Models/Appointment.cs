using System.ComponentModel.DataAnnotations;

namespace SzervizPont.Models;

public class Appointment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int CarId { get; set; }
    public Car? Car { get; set; }
    public int ServiceItemId { get; set; }
    public ServiceItem? ServiceItem { get; set; }
    public DateTime StartAt { get; set; }

    [MaxLength(30)]
    public string Status { get; set; } = AppointmentStatuses.Pending;

    [MaxLength(500)]
    public string? Note { get; set; }
}
