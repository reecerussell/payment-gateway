using System.Net;

namespace Payments.Abstractions.Exceptions;

public class InternalServerErrorException : ApiException
{
    public InternalServerErrorException() : this("An internal server error occured!")
    {
    }
    
    public InternalServerErrorException(string message)
        : base(ExceptionTypes.InternalServerError, message, HttpStatusCode.InternalServerError)
    {
    }
}