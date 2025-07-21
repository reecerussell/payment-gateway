using System.Net;

namespace Payments.Exceptions;

public abstract class ApiException : Exception
{
    public string Type { get; }
    public string? ParamName { get; }
    public HttpStatusCode StatusCode { get; }

    protected ApiException(string type, string message, HttpStatusCode statusCode)
        : this(type, message, null, statusCode)
    {
    }

    protected ApiException(string type, string message, string? paramName, HttpStatusCode statusCode)
        : base(message)
    {
        Type = type;
        ParamName = paramName;
        StatusCode = statusCode;
    }
}