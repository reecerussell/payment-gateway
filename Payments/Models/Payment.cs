using Payments.Abstractions;
using Payments.Abstractions.Exceptions;

namespace Payments.Models;

public class Payment
{
    public const int MinCardNumberLength = 14;
    public const int MaxCardNumberLength = 19;
    public const int MinExpiryMonth = 1;
    public const int MaxExpiryMonth = 12;
    public const int RequiredCurrencyCodeLength = 3;
    public const int MinCvvLength = 3;
    public const int MaxCvvLength = 4;
    
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
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            throw new ValidationException("Card number is a required field.", nameof(cardNumber));
        }

        if (cardNumber.Length < MinCardNumberLength || cardNumber.Length > MaxCardNumberLength)
        {
            throw new ValidationException($"Card number must be between {MinCardNumberLength} and {MaxCardNumberLength} digits long.", nameof(cardNumber));
        }

        if (!cardNumber.All(char.IsDigit))
        {
            throw new ValidationException("Card number can only consist of digits.");
        }
        
        LastFourCardDigits = cardNumber[^4..];
    }

    private void SetExpiry(int expiryMonth, int expiryYear, DateTime currentDate)
    {
        if (expiryMonth < MinExpiryMonth || expiryMonth > MaxExpiryMonth)
        {
            throw new ValidationException($"Expiry month must be between {MinExpiryMonth} and {MaxExpiryMonth}.", nameof(expiryMonth));
        }

        var expiry = new DateTime(expiryYear, expiryMonth, 1);
        if (expiry < currentDate)
        {
            throw new ValidationException("The card has expired.");
        }
        
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
    }

    private void SetCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ValidationException("Currency is a required field.", nameof(currency));
        }

        if (currency.Length != RequiredCurrencyCodeLength)
        {
            throw new ValidationException($"Currency must contain only {RequiredCurrencyCodeLength} characters.", nameof(currency));
        }

        if (!CurrencyCodes.Iso4217.TryGetValue(currency.ToUpper(), out var normalizedCode))
        {
            throw new ValidationException("Currency must be a valid ISO 4217 currency code.", nameof(currency));
        }
        
        Currency = normalizedCode;
    }

    private void SetAmount(int amount)
    {
        Amount = amount;
    }

    private void ValidateCvv(string cvv)
    {
        if (string.IsNullOrWhiteSpace(cvv))
        {
            throw new ValidationException("CVV is a required field.", nameof(cvv));
        }

        if (cvv.Length < MinCvvLength || cvv.Length > MaxCvvLength)
        {
            throw new ValidationException($"CVV must be between {MinCvvLength} and {MaxCvvLength} digits long.", nameof(cvv));
        }
        
        if (!cvv.All(char.IsDigit))
        {
            throw new ValidationException("CVV can only consist of digits.");
        }
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
        payment.ValidateCvv(request.Cvv);

        return payment;
    }
}