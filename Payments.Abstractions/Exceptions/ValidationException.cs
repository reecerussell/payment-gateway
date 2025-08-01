using System.Net;

namespace Payments.Abstractions.Exceptions;

public class ValidationException : ApiException
{
    public ValidationException(string message, string? paramName = null)
        : base(ExceptionTypes.Validation, message, paramName, HttpStatusCode.BadRequest)
    {
    }
}