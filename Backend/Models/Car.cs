using System.ComponentModel.DataAnnotations;

namespace SzervizPont.Models;

public class Car
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(20)]
    public string PlateNumber { get; set; } = "";

    [MaxLength(80)]
    public string Brand { get; set; } = "";

    [MaxLength(80)]
    public string Model { get; set; } = "";

    public int Year { get; set; }
    public List<Appointment> Appointments { get; set; } = new List<Appointment>();
}
