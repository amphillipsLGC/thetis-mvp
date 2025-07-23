namespace Thetis.Users.Domain;

internal class UsernameAlreadyInUseException : Exception
{
    public UsernameAlreadyInUseException()
    {
    }

    public UsernameAlreadyInUseException(string username)
        : base($"Username '{username}' is already in use.")
    {
    }

    public UsernameAlreadyInUseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
    
}

internal class EmailAlreadyInUseException : Exception
{
    public EmailAlreadyInUseException()
    {
    }

    public EmailAlreadyInUseException(string email)
        : base($"Email '{email}' is already in use.")
    {
    }

    public EmailAlreadyInUseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}