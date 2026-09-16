namespace CarRental.Domain.Exceptions;

public class CarNotAvailableException : DomainException
{
    public CarNotAvailableException(int carId, DateTime startDate, DateTime endDate)
        : base($"Car {carId} is not available between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}.")
    {
    }
}
