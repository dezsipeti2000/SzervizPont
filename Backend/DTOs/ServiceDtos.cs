namespace SzervizPont.DTOs;

public class ServiceRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Price { get; set; }
    public int EstimatedMinutes { get; set; }
}
