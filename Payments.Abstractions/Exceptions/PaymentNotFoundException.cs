using System.Net;

namespace Payments.Abstractions.Exceptions;

public class PaymentNotFoundException : ApiException
{
    private const string MessageText = "The specified payment could not be found.";
    
    public PaymentNotFoundException()
        : base(ExceptionTypes.PaymentNotFound, MessageText, HttpStatusCode.NotFound)
    {
    }
}