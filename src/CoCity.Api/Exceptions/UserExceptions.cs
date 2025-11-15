namespace CoCity.Api.Exceptions;

public class UpdateFailedException(string reason)
    : Exception($"Failed to update user profile: {reason}.")
{ }
