using Payments.Enums;

namespace Payments.Dtos;

public record PaymentResponse
{
    /// <summary>
    /// The ID of the payment represented as a UUID.
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// The status of the payment.
    /// </summary>
    public required PaymentStatus Status { get; init; }
    
    /// <summary>
    /// The last four digits of the payment card.
    /// </summary>
    public required string LastFourCardDigits { get; init; }
    
    /// <summary>
    /// The expiry month of the payment card.
    /// </summary>
    public required int ExpiryMonth { get; init; }
    
    /// <summary>
    /// The expiry year of the payment card.
    /// </summary>
    public required int ExpiryYear { get; init; }
    
    /// <summary>
    /// The ISO currency code of the payment.
    /// </summary>
    public required string Currency { get; init; }
    
    /// <summary>
    /// The amount of the payment represented in the minor currency unit. For example,
    /// if the currency was GBP, £0.01 would be supplied as 1 and £10.50 would be supplied
    /// as 1050.
    /// </summary>
    public required int Amount { get; init; } 
}