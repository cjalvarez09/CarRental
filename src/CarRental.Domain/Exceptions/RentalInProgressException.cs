namespace CarRental.Domain.Exceptions;

public class RentalInProgressException : DomainException
{
    public RentalInProgressException(int rentalId)
        : base($"Rental {rentalId} can't be cancelled because it has already started.")
    {
    }
}
