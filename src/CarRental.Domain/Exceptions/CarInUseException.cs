namespace CarRental.Domain.Exceptions;

public class CarInUseException : DomainException
{
    public CarInUseException(int carId)
        : base($"Car {carId} cannot be deleted because it has rentals associated with it.")
    {
    }
}
