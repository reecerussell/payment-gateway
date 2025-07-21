using Payments.Dtos;
using Payments.Enums;

namespace Payments.Models;

public class Payment
{
    public Guid Id { get; }
    public DateTime DateCreated { get; set; }
    public PaymentStatus Status { get; }
    public string LastFourCardDigits { get; }
    public int ExpiryMonth { get; }
    public int ExpiryYear { get; }
    public string Currency { get; }
    public int Amount { get; }
    public string? AuthorizationCode { get; }

    private Payment(DateTime currentDate)
    {
        Id = Guid.NewGuid();
        DateCreated = currentDate;
        
        // Initiate the payment as decline, which can later be authorized.
        Status = PaymentStatus.Declined;
    }

    private void SetLastFourDigits(string cardNumber)
    {
        
    }

    private void SetExpiry(int month, int year, DateTime currentDate)
    {
        
    }

    private void SetCurrency(string currency)
    {
        
    }

    private void SetAmount(int amount)
    {
        
    }

    public static Payment Create(PaymentRequest request, DateTime currentDate)
    {
        var payment = new Payment(currentDate);
        payment.SetLastFourDigits(request.CardNumber);
        payment.SetExpiry(request.ExpiryMonth, request.ExpiryYear, currentDate);
        payment.SetCurrency(request.Currency);
        payment.SetAmount(request.Amount);

        return payment;
    }
}