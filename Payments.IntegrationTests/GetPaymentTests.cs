using Payments.Abstractions;
using Payments.Abstractions.Exceptions;

namespace Payments.IntegrationTests;

public class GetPaymentTests
{
    private PaymentResponse _payment;

    /// <summary>
    /// These tests use this one time setup to seed a payment into the in-memory collection.
    /// This could be improved if we used an external database which we could seed payment
    /// records into as we could keep our tests isolated to the pure GET-only endpoints/logic.
    /// </summary>
    [OneTimeSetUp]
    public async Task SetupAsync()
    {
        var request = new PaymentRequest
        {
            CardNumber = "123456789101494",
            ExpiryMonth = DateTime.UtcNow.AddMonths(2).Month,
            ExpiryYear = DateTime.UtcNow.AddMonths(2).Year,
            Cvv = "499",
            Amount = 399,
            Currency = "AUD"
        };

        var result = await Helpers.PostPaymentAsync(request);
        _payment = result.Data!;
    }

    [Test]
    public async Task WherePaymentExists_ReturnsPayment()
    {
        var result = await Helpers.GetPaymentAsync(_payment.Id);
        
        Assert.That(result.Error, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Id, Is.EqualTo(_payment.Id));
        Assert.That(result.Data.Status, Is.EqualTo(_payment.Status));
        Assert.That(result.Data.LastFourCardDigits, Is.EqualTo(_payment.LastFourCardDigits));
        Assert.That(result.Data.ExpiryMonth, Is.EqualTo(_payment.ExpiryMonth));
        Assert.That(result.Data.ExpiryYear, Is.EqualTo(_payment.ExpiryYear));
        Assert.That(result.Data.Amount, Is.EqualTo(_payment.Amount));
        Assert.That(result.Data.Currency, Is.EqualTo(_payment.Currency));
    }

    [Test]
    public async Task WherePaymentDoesNotExist_ReturnsNotFoundError()
    {
        var result = await Helpers.GetPaymentAsync(Guid.NewGuid().ToString()); // Using new Guid as random ID
        
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Error.Type, Is.EqualTo(ExceptionTypes.PaymentNotFound));
    }
}