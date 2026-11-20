using System.Net;

namespace ReservationService.Exceptions;

/// <summary>
/// Thrown when an inter-service communication failure occurs with a downstream microservice.
/// </summary>
public class DownstreamServiceException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public DownstreamServiceException(string message, HttpStatusCode? statusCode = null)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public DownstreamServiceException(string message, Exception innerException, HttpStatusCode? statusCode = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
