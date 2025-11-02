namespace CoCity.Api.Exceptions;

public class UserAlreadyExistsException(string userName)
    : Exception($"A user with the username '{userName}' already exists.")
{ }

public class InvalidCredentialsException()
    : Exception("The provided username or password is incorrect.")
{ }