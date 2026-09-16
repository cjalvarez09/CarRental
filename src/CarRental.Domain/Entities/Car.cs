using CarRental.Domain.Common;

namespace CarRental.Domain.Entities;

public class Car : ISoftDelete
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public List<Service> Services { get; set; } = [];
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
}
