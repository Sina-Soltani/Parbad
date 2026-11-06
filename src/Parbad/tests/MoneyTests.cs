using NUnit.Framework;

namespace Parbad.Tests;

[TestFixture]
public class MoneyTests
{
    private const decimal ExpectedAmount = 1000;

    [Test]
    public void Amount_Must_Be_Equal_With_ExpectedAmount()
    {
        var instance1 = new Money(ExpectedAmount);
        var instance2 = Money.Parse(ExpectedAmount.ToString());
        Money.TryParse(ExpectedAmount.ToString(), out var instance3);

        Assert.That(instance1.Value, Is.EqualTo(ExpectedAmount));
        Assert.That(instance2.Value, Is.EqualTo(ExpectedAmount));
        Assert.That(instance3.Value, Is.EqualTo(ExpectedAmount));
    }

    [Test]
    public void ToString_Value_Must_Be_Equal_With_Amount()
    {
        var instance = new Money(ExpectedAmount);

        Assert.That(instance.ToString(), Is.EqualTo(ExpectedAmount.ToString()));
    }

    [Test]
    public void Casted_Value_Must_Be_Equal_With_Amount()
    {
        var amount = (long)new Money(ExpectedAmount);

        Assert.That(amount, Is.EqualTo(ExpectedAmount));
    }

    [Test]
    public void Added_Amount_Must_Be_Equal_With_2000()
    {
        var instance = new Money(ExpectedAmount).AddAmount(ExpectedAmount);

        Assert.That((decimal)instance, Is.EqualTo(ExpectedAmount * 2));
    }

    [Test]
    public void Compare_Method_Works()
    {
        var instance = new Money(ExpectedAmount);
        var instance2 = new Money(ExpectedAmount);

        Assert.That(instance.CompareTo(instance2), Is.EqualTo(0));
    }

    [Test]
    public void Equatable_Works()
    {
        var money = new Money(10);

        Assert.That(money.Equals(10L), Is.True);
        Assert.That(money.Equals(10M), Is.True);
        Assert.That(new Money(10), Is.EqualTo(money));
    }
}