using JK.Playground.Services;

namespace JK.Playground.Test;

[TestFixture]
public class DiscountServiceTest
{
    private DiscountService _service;
    [SetUp]
    public void Setup()
    {
        _service = new DiscountService();
    }

    [TestCase(-1)]
    [TestCase(0)]
    public void Apply_WrongAmount_ThrowOutOfRangeException(decimal amount)
    {
        TestDelegate act = () => _service.Apply(amount, false);
        Assert.Throws<ArgumentException>(act, $"Amount must be greater than zero. Provided: {amount}");
    }

    [Test]
    public void Apply_ValidAmount_ReturnDiscount()
    {
        var finalAmount = _service.Apply(10, true);
        Assert.AreEqual(9, finalAmount);
    }

    [Test]
    public void Apply_ValidAmount_ReturnNoDiscount()
    {
        var finalAmount = _service.Apply(10, false);
        Assert.AreEqual(10, finalAmount);
    }

}