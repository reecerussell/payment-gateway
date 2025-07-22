namespace Payments.Abstractions;

public record ApiError
{
    public required string Type { get; init; }
    public required string Message { get; init; }
    public string? ParamName { get; init; }
}