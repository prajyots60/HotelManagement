using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using HotelManagementService.Exceptions;

namespace HotelManagementService.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var isUniqueConstraintViolation =
            IsUniqueConstraintViolation(exception);

        var (statusCode, title, detail) = exception switch
        {
            ConflictException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message),

            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Not Found",
                exception.Message),

            ForbiddenException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                exception.Message),

            _ when isUniqueConstraintViolation => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "A record with the same unique value already exists."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                "The server encountered an unexpected error. Please try again later.")
        };

        if (exception is ConflictException
            or NotFoundException
            or ForbiddenException
            || isUniqueConstraintViolation)
        {
            _logger.LogWarning(
                "Operation could not be completed. StatusCode: {StatusCode}, TraceId: {TraceId}",
                statusCode,
                httpContext.TraceIdentifier);
        }
        else
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred. TraceId: {TraceId}",
                httpContext.TraceIdentifier);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });
    }

    private static bool IsUniqueConstraintViolation(Exception exception)
    {
        if (exception is not DbUpdateException)
        {
            return false;
        }

        var current = exception.InnerException;

        while (current is not null)
        {
            if (current is SqlException sqlException
                && (sqlException.Number == 2601
                    || sqlException.Number == 2627))
            {
                return true;
            }

            current = current.InnerException;
        }

        return false;
    }
}
