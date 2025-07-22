namespace Payments.Bank;

public record ErrorResponse
{
    public required string ErrorMessage { get; set; }
}