namespace HouseMaintenance.Application.Exceptions;

public sealed class ResourceNotFoundException(string message) : Exception(message);
