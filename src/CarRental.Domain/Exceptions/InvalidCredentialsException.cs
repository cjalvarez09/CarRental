namespace CarRental.Domain.Exceptions;

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("The username or password is incorrect.")
    {
    }
}
