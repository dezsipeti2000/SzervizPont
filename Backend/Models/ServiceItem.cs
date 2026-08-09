using System.ComponentModel.DataAnnotations;

namespace SzervizPont.Models;

public class ServiceItem
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = "";

    [MaxLength(500)]
    public string Description { get; set; } = "";

    public int Price { get; set; }
    public int EstimatedMinutes { get; set; }
    public List<Appointment> Appointments { get; set; } = new List<Appointment>();
}
