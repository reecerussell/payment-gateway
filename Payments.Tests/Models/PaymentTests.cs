using Payments.Abstractions;
using Payments.Abstractions.Exceptions;
using Payments.Models;

namespace Payments.Tests.Models;

public class PaymentTests
{
    [Test]
    public void Create_GivenValidInputs_ReturnsPaymentPopulatedWithData()
    {
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "324923479237135",
            Amount = 488,
            Currency = "GBP",
            Cvv = "223",
            ExpiryMonth = 3,
            ExpiryYear = 2026,
        };

        var payment = Payment.Create(request, currentDate);
        
        Assert.That(payment.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(payment.LastFourCardDigits, Is.EqualTo(request.CardNumber[^4..]));
        Assert.That(payment.ExpiryMonth, Is.EqualTo(request.ExpiryMonth));
        Assert.That(payment.ExpiryYear, Is.EqualTo(request.ExpiryYear));
        Assert.That(payment.Currency, Is.EqualTo(request.Currency));
        Assert.That(payment.Amount, Is.EqualTo(request.Amount));
        Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Declined));
        Assert.That(payment.DateCreated, Is.EqualTo(currentDate));
        Assert.That(payment.AuthorizationCode, Is.Null);
    }

    [Test]
    public void Create_GivenEmptyCardNumber_ThrowsValidationException()
    {
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "",
            Amount = 488,
            Currency = "GBP",
            Cvv = "223",
            ExpiryMonth = 3,
            ExpiryYear = 2026,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_GivenShortCardNumber_ThrowsValidationException()
    {
        var cardNumber = "";
        for (var i = 0; i < Payment.MinCardNumberLength - 1; i++)
        {
            cardNumber += "1";
        }
        
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = cardNumber,
            Amount = 488,
            Currency = "GBP",
            Cvv = "223",
            ExpiryMonth = 3,
            ExpiryYear = 2026,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_GivenLongCardNumber_ThrowsValidationException()
    {
        var cardNumber = "";
        for (var i = 0; i < Payment.MaxCardNumberLength + 1; i++)
        {
            cardNumber += "1";
        }
        
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = cardNumber,
            Amount = 488,
            Currency = "GBP",
            Cvv = "223",
            ExpiryMonth = 3,
            ExpiryYear = 2026,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_GivenCardNumberWithNonDigits_ThrowsValidationException()
    {
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "3247324342h3838",
            Amount = 488,
            Currency = "GBP",
            Cvv = "223",
            ExpiryMonth = 3,
            ExpiryYear = 2026,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_WhereExpiryMonthIsTooLittle_ThrowsValidationException()
    {
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "32473243423838",
            Amount = 488,
            Currency = "GBP",
            Cvv = "223",
            ExpiryMonth = Payment.MinExpiryMonth - 1,
            ExpiryYear = 2027,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_WhereExpiryMonthIsTooHigh_ThrowsValidationException()
    {
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "32473243423838",
            Amount = 488,
            Currency = "GBP",
            Cvv = "223",
            ExpiryMonth = Payment.MaxExpiryMonth + 1,
            ExpiryYear = 2027,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_WhereExpiryDateIsInThePast_ThrowsValidationException()
    {
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "32473243423838",
            Amount = 488,
            Currency = "GBP",
            Cvv = "223",
            ExpiryMonth = 5,
            ExpiryYear = 2024,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_WhereCurrencyIsNotSet_ThrowsValidationException()
    {
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "32473243423838",
            Amount = 488,
            Currency = "",
            Cvv = "223",
            ExpiryMonth = 5,
            ExpiryYear = 2027,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_WhereCurrencyNotTheRequiredLength_ThrowsValidationException()
    {
        var currency = "";
        for (var i = 0; i < Payment.RequiredCurrencyCodeLength + 1; i++)
        {
            currency += "A";
        }
        
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "32473243423838",
            Amount = 488,
            Currency = currency,
            Cvv = "223",
            ExpiryMonth = 5,
            ExpiryYear = 2027,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_WhereCurrencyIsNotValid_ThrowsValidationException()
    {
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "32473243423838",
            Amount = 488,
            Currency = "ZzX",
            Cvv = "223",
            ExpiryMonth = 5,
            ExpiryYear = 2027,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_GivenCasingVariantCurrency_ReturnsPaymentWithNormalizedCode()
    {
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "324923479237135",
            Amount = 488,
            Currency = "Gbp",
            Cvv = "223",
            ExpiryMonth = 3,
            ExpiryYear = 2026,
        };

        var payment = Payment.Create(request, currentDate);
        
        Assert.That(payment.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(payment.LastFourCardDigits, Is.EqualTo(request.CardNumber[^4..]));
        Assert.That(payment.ExpiryMonth, Is.EqualTo(request.ExpiryMonth));
        Assert.That(payment.ExpiryYear, Is.EqualTo(request.ExpiryYear));
        Assert.That(payment.Currency, Is.EqualTo(request.Currency.ToUpper()));
        Assert.That(payment.Amount, Is.EqualTo(request.Amount));
        Assert.That(payment.Status, Is.EqualTo(PaymentStatus.Declined));
        Assert.That(payment.DateCreated, Is.EqualTo(currentDate));
        Assert.That(payment.AuthorizationCode, Is.Null);
    }
    
    [Test]
    public void Create_WhereCvvIsNotSet_ThrowsValidationException()
    {
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "32473243423838",
            Amount = 488,
            Currency = "GBP",
            Cvv = "",
            ExpiryMonth = 5,
            ExpiryYear = 2027,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_WhereCvvIsTooShort_ThrowsValidationException()
    {
        var cvv = "";
        for (var i = 0; i < Payment.MinCvvLength - 1; i++)
        {
            cvv += "1";
        }
        
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "32473243423838",
            Amount = 488,
            Currency = "GBP",
            Cvv = cvv,
            ExpiryMonth = 5,
            ExpiryYear = 2027,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_WhereCvvIsTooLong_ThrowsValidationException()
    {
        var cvv = "";
        for (var i = 0; i < Payment.MaxCvvLength + 1; i++)
        {
            cvv += "1";
        }
        
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "32473243423838",
            Amount = 488,
            Currency = "GBP",
            Cvv = cvv,
            ExpiryMonth = 5,
            ExpiryYear = 2027,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_WhereCvvContainsNonDigits_ThrowsValidationException()
    {
        var currentDate = new DateTime(2025, 7, 22);
        var request = new PaymentRequest
        {
            CardNumber = "32473243423838",
            Amount = 488,
            Currency = "GBP",
            Cvv = "Ab1",
            ExpiryMonth = 5,
            ExpiryYear = 2027,
        };

        Assert.Throws<ValidationException>(() => Payment.Create(request, currentDate));
    }
    
    [Test]
    public void Create_GivenValidCurrencies_DoesNotThrow()
    {
        foreach (var code in CurrencyCodes.Iso4217)
        {
            var currentDate = new DateTime(2025, 7, 22);
            var request = new PaymentRequest
            {
                CardNumber = "32473243423838",
                Amount = 488,
                Currency = code,
                Cvv = "223",
                ExpiryMonth = 5,
                ExpiryYear = 2027,
            };

            var payment = Payment.Create(request, currentDate);
            
            Assert.That(payment.Currency, Is.EqualTo(code));
        }
    }
}