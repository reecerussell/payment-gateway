using Payments.Abstractions.Exceptions;

namespace Payments.Bank;

public class BankException : InternalServerErrorException
{
    public BankException(string message)
        : base($"An error occured while operating with the Bank: {message}")
    {
    }
}