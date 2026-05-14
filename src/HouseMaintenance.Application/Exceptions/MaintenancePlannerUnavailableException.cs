namespace HouseMaintenance.Application.Exceptions;

public sealed class MaintenancePlannerUnavailableException(string message) : Exception(message);
