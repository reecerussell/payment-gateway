using Payments.Abstractions;
using Payments.Abstractions.Exceptions;

namespace Payments.IntegrationTests;

public class PostPaymentTests
{
    [Test]
    public async Task GivenOddEndingCardNumber_ReturnsAuthorizedPayment()
    {
        var request = new PaymentRequest
        {
            CardNumber = "123456789101213",
            ExpiryMonth = DateTime.UtcNow.AddMonths(1).Month,
            ExpiryYear = DateTime.UtcNow.AddMonths(1).Year,
            Cvv = "192",
            Amount = 1000,
            Currency = "GBP"
        };

        var result = await Helpers.PostPaymentAsync(request);

        Assert.That(result.Error, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Id, Is.Not.Empty);
        Assert.That(result.Data.LastFourCardDigits, Is.EqualTo(request.CardNumber[^4..]));
        Assert.That(result.Data.ExpiryMonth, Is.EqualTo(request.ExpiryMonth));
        Assert.That(result.Data.ExpiryYear, Is.EqualTo(request.ExpiryYear));
        Assert.That(result.Data.Amount, Is.EqualTo(request.Amount));
        Assert.That(result.Data.Currency, Is.EqualTo(request.Currency));
        Assert.That(result.Data.Status, Is.EqualTo(PaymentStatus.Authorized));
    }
    
    [Test]
    public async Task GivenEvenEndingCardNumber_ReturnsDeclinePayment()
    {
        var request = new PaymentRequest
        {
            CardNumber = "123456789101214",
            ExpiryMonth = DateTime.UtcNow.AddMonths(2).Month,
            ExpiryYear = DateTime.UtcNow.AddMonths(2).Year,
            Cvv = "332",
            Amount = 500,
            Currency = "USD"
        };

        var result = await Helpers.PostPaymentAsync(request);

        Assert.That(result.Error, Is.Null);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data.Id, Is.Not.Empty);
        Assert.That(result.Data.LastFourCardDigits, Is.EqualTo(request.CardNumber[^4..]));
        Assert.That(result.Data.ExpiryMonth, Is.EqualTo(request.ExpiryMonth));
        Assert.That(result.Data.ExpiryYear, Is.EqualTo(request.ExpiryYear));
        Assert.That(result.Data.Amount, Is.EqualTo(request.Amount));
        Assert.That(result.Data.Currency, Is.EqualTo(request.Currency));
        Assert.That(result.Data.Status, Is.EqualTo(PaymentStatus.Declined));
    }
    
    [Test]
    public async Task GivenCardNumberEndingInZero_ReturnsError()
    {
        var request = new PaymentRequest
        {
            CardNumber = "1234567891014940",
            ExpiryMonth = DateTime.UtcNow.AddMonths(2).Month,
            ExpiryYear = DateTime.UtcNow.AddMonths(2).Year,
            Cvv = "499",
            Amount = 399,
            Currency = "AUD"
        };

        var result = await Helpers.PostPaymentAsync(request);

        Assert.That(result.Data, Is.Null);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Type, Is.EqualTo(ExceptionTypes.InternalServerError));
    }
    
    [Test]
    public async Task GivenInvalidInput_ReturnsError()
    {
        var request = new PaymentRequest
        {
            CardNumber = "not a card number",
            ExpiryMonth = DateTime.UtcNow.AddMonths(-2).Month,
            ExpiryYear = DateTime.UtcNow.AddMonths(-2).Year,
            Cvv = "4994444",
            Amount = -2340,
            Currency = "currency"
        };

        var result = await Helpers.PostPaymentAsync(request);

        Assert.That(result.Data, Is.Null);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Type, Is.EqualTo(ExceptionTypes.Validation));
    }
}