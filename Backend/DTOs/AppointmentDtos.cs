namespace SzervizPont.DTOs;

public class AppointmentCreateRequest
{
    public int CarId { get; set; }
    public int ServiceItemId { get; set; }
    public DateTime StartAt { get; set; }
    public string Note { get; set; } = "";
}

public class AppointmentUpdateRequest
{
    public DateTime StartAt { get; set; }
    public string Note { get; set; } = "";
}

public class AppointmentStatusUpdateRequest
{
    public string Status { get; set; } = "";
}
