using Payments.Abstractions;

namespace Payments.Models;

public class Payment
{
    public Guid Id { get; }
    public DateTime DateCreated { get; }
    public PaymentStatus Status { get; private set; }
    public string LastFourCardDigits { get; private set; }
    public int ExpiryMonth { get; private set; }
    public int ExpiryYear { get; private set; }
    public string Currency { get; private set; }
    public int Amount { get; private set; }
    public string? AuthorizationCode { get; private set; }

    private Payment(DateTime currentDate)
    {
        Id = Guid.NewGuid();
        DateCreated = currentDate;
        
        // Initiate the payment as decline, which can later be authorized.
        Status = PaymentStatus.Declined;
    }

    private void SetLastFourCardDigits(string cardNumber)
    {
        LastFourCardDigits = cardNumber[^4..];
    }

    private void SetExpiry(int month, int year, DateTime currentDate)
    {
        ExpiryMonth = month;
        ExpiryYear = year;
    }

    private void SetCurrency(string currency)
    {
        Currency = currency;
    }

    private void SetAmount(int amount)
    {
        Amount = amount;
    }

    public void MarkAsAuthorized(string authorizationCode)
    {
        Status = PaymentStatus.Authorized;
        AuthorizationCode = authorizationCode;
    }

    public static Payment Create(PaymentRequest request, DateTime currentDate)
    {
        var payment = new Payment(currentDate);
        payment.SetLastFourCardDigits(request.CardNumber);
        payment.SetExpiry(request.ExpiryMonth, request.ExpiryYear, currentDate);
        payment.SetCurrency(request.Currency);
        payment.SetAmount(request.Amount);

        return payment;
    }
}