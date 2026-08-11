using System.Net;

namespace EPR.LiveService.FunctionApp.Services;

public class OrganisationServiceException : Exception
{
    public OrganisationServiceException(
        string message,
        HttpStatusCode? statusCode = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode? StatusCode { get; }
}
