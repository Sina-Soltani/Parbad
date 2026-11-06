using System;
using System.Globalization;
using NUnit.Framework;

// These tests check Money's own operators and Equals overloads. The suggested constraints (Is.LessThan,
// Is.EqualTo, ...) compare with IComparable and NUnit's equality rules, so they would not call them.
#pragma warning disable NUnit2043, NUnit2010

namespace Parbad.PaymentFlow.Tests.Core;

/// <summary>
/// Amount comparisons that payment checks depend on.
/// </summary>
[TestFixture]
public class MoneyTests
{
    [Test]
    public void Smaller_amount_is_less_than_larger_amount()
    {
        Assert.That(new Money(5) < new Money(6), Is.True);
        Assert.That(new Money(6) < new Money(5), Is.False);
    }

    [Test]
    public void Greater_than_and_or_equal_operators_are_consistent()
    {
        var five = new Money(5);
        var sameAmount = new Money(5);

        Assert.That(five > sameAmount, Is.False);
        Assert.That(five >= sameAmount, Is.True);
        Assert.That(five <= sameAmount, Is.True);
    }

    [Test]
    public void Equal_amounts_are_not_less_than_each_other()
    {
        var five = new Money(5);
        var sameAmount = new Money(5);

        Assert.That(five < sameAmount, Is.False, "Money(5) < Money(5) returned true.");
    }

    [Test]
    public void Amount_with_a_fraction_is_not_equal_to_its_truncated_value()
    {
        Assert.That(new Money(10.5m).Equals(10L), Is.False, "Money(10.5).Equals(10L) returned true.");
        Assert.That(new Money(10.5m).Equals((object)10L), Is.False);
        Assert.That(new Money(10m).Equals(10L), Is.True);
    }

    [Test]
    public void Nullable_overloads_compare_the_value()
    {
        Assert.That(new Money(10).Equals((Money?)new Money(10)), Is.True);
        Assert.That(new Money(10).Equals((long?)10), Is.True);
        Assert.That(new Money(10).Equals((decimal?)10m), Is.True);
        Assert.That(new Money(10).Equals((long?)null), Is.False);
        Assert.That(new Money(10).Equals((Money?)new Money(11)), Is.False);
    }

    [Test]
    public void Equal_amounts_have_the_same_hash_code()
    {
        Assert.That(new Money(10.00m).GetHashCode(), Is.EqualTo(new Money(10m).GetHashCode()));
    }

    [Test]
    public void Whole_amount_converts_to_long()
    {
        Assert.That((long)new Money(1_250_000m), Is.EqualTo(1_250_000L));
    }

    [Test]
    public void Amount_with_a_fraction_cannot_be_converted_to_long()
    {
        Assert.Throws<InvalidOperationException>(() => _ = (long)new Money(10.5m));
    }

    [TestCase("fa-IR")]
    [TestCase("de-DE")]
    [TestCase("en-US")]
    public void Parse_uses_the_invariant_culture(string culture)
    {
        var current = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);

            Assert.That(Money.Parse("1000.5"), Is.EqualTo(new Money(1000.5m)));
            Assert.That(Money.TryParse("1250000", out var money), Is.True);
            Assert.That(money.Value, Is.EqualTo(1_250_000m));
            Assert.That(Money.TryParse("abc", out _), Is.False);
            Assert.Throws<FormatException>(() => Money.Parse("abc"));
        }
        finally
        {
            CultureInfo.CurrentCulture = current;
        }
    }
}
