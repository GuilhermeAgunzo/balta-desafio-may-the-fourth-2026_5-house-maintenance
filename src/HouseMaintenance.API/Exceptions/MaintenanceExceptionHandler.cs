using HouseMaintenance.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HouseMaintenance.API.Exceptions;

public sealed class MaintenanceExceptionHandler(ILogger<MaintenanceExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail, logAsError) = exception switch
        {
            RequestValidationException => (StatusCodes.Status400BadRequest, "Requisição inválida", exception.Message, false),
            ResourceNotFoundException => (StatusCodes.Status404NotFound, "Missão não encontrada", exception.Message, false),
            MaintenancePlannerUnavailableException => (StatusCodes.Status503ServiceUnavailable, "Agente indisponível", exception.Message, true),
            MaintenancePlanContractException => (StatusCodes.Status502BadGateway, "Resposta inválida da IA", exception.Message, true),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno", "Ocorreu uma falha inesperada ao processar a missão.", true)
        };

        if (logAsError)
        {
            logger.LogError(exception, "Erro ao processar a missão de manutenção.");
        }
        else
        {
            logger.LogWarning(exception, "Falha tratada ao processar a missão de manutenção.");
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
