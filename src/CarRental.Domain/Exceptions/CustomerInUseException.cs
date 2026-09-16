namespace CarRental.Domain.Exceptions;

public class CustomerInUseException : DomainException
{
    public CustomerInUseException(int customerId)
        : base($"Customer {customerId} cannot be deleted because it has rentals associated with it.")
    {
    }
}
