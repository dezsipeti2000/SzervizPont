using System.ComponentModel.DataAnnotations;

namespace SzervizPont.Models;

public class User
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = "";

    [MaxLength(150)]
    public string Email { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    [MaxLength(20)]
    public string Role { get; set; } = Roles.User;

    public List<Car> Cars { get; set; } = new List<Car>();
    public List<Appointment> Appointments { get; set; } = new List<Appointment>();
}
