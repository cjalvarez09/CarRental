using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities;

public class Rental
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int CarId { get; set; }
    public Car Car { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public RentalStatus Status { get; set; } = RentalStatus.Active;
}
