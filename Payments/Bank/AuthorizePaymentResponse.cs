namespace Payments.Bank;

public record AuthorizePaymentResponse
{
    public required bool Authorized { get; init; }
    public required string AuthorizationCode { get; init; }
}