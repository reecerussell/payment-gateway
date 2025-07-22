namespace Payments.Abstractions;

public record PaymentRequest
{
    /// <summary>
    /// The card number to take payment from. This is a required field and must be
    /// between 14 and 19 characters long, containing only numeric fields.
    /// </summary>
    public required string CardNumber { get; init; }
    
    /// <summary>
    /// The expiry month of the payment card. This is a required field and must be
    /// a value between 1 and 12.
    /// </summary>
    public required int ExpiryMonth { get; init; }
    
    /// <summary>
    /// The expiry year of the payment card. This is a required field and in combination
    /// with the expiry month, must be a date in the future.
    /// </summary>
    public required int ExpiryYear { get; init; }
    
    /// <summary>
    /// The currency of the payment. This is a required field and must be a valid
    /// ISO currency code.
    /// </summary>
    public required string Currency { get; init; }
    
    /// <summary>
    /// The amount of the payment represented in the minor currency unit. For example,
    /// if the currency was GBP, £0.01 would be supplied as 1 and £10.50 would be supplied
    /// as 1050. This is a required field.
    /// </summary>
    public required int Amount { get; init; } 
    
    /// <summary>
    /// The CVV of the payment card. This is a required field and must be between 3 and 4
    /// characters long, while only containing numeric characters.
    /// </summary>
    public required string Cvv { get; init; }
}