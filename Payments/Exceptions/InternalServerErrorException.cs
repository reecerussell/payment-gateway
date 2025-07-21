using System.Net;

namespace Payments.Exceptions;

public class InternalServerErrorException : ApiException
{
    public InternalServerErrorException(string message)
        : base(ExceptionTypes.InternalServerError, message, HttpStatusCode.InternalServerError)
    {
    }
}