namespace CarRental.Domain.Entities;

public class Car
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public List<Service> Services { get; set; } = [];
}
