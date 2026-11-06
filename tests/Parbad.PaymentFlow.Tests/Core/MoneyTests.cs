using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parbad.PaymentFlow.Tests.Core;

/// <summary>
/// Amount comparisons that payment checks depend on.
/// </summary>
[TestClass]
public class MoneyTests
{
    [TestMethod]
    public void Smaller_amount_is_less_than_larger_amount()
    {
        Assert.IsTrue(new Money(5) < new Money(6));
        Assert.IsFalse(new Money(6) < new Money(5));
    }

    [TestMethod]
    public void Greater_than_and_or_equal_operators_are_consistent()
    {
        Assert.IsFalse(new Money(5) > new Money(5));
        Assert.IsTrue(new Money(5) >= new Money(5));
        Assert.IsTrue(new Money(5) <= new Money(5));
    }

    [TestMethod]
    public void Equal_amounts_are_not_less_than_each_other()
    {
        Assert.IsFalse(new Money(5) < new Money(5), "Money(5) < Money(5) returned true.");
    }

    [TestMethod]
    public void Amount_with_a_fraction_is_not_equal_to_its_truncated_value()
    {
        Assert.IsFalse(new Money(10.5m).Equals(10L), "Money(10.5).Equals(10L) returned true.");
        Assert.IsFalse(new Money(10.5m).Equals((object)10L));
        Assert.IsTrue(new Money(10m).Equals(10L));
    }

    [TestMethod]
    public void Nullable_overloads_compare_the_value()
    {
        Assert.IsTrue(new Money(10).Equals((Money?)new Money(10)));
        Assert.IsTrue(new Money(10).Equals((long?)10));
        Assert.IsTrue(new Money(10).Equals((decimal?)10m));
        Assert.IsFalse(new Money(10).Equals((long?)null));
        Assert.IsFalse(new Money(10).Equals((Money?)new Money(11)));
    }

    [TestMethod]
    public void Equal_amounts_have_the_same_hash_code()
    {
        Assert.AreEqual(new Money(10m).GetHashCode(), new Money(10.00m).GetHashCode());
    }

    [TestMethod]
    public void Whole_amount_converts_to_long()
    {
        Assert.AreEqual(1_250_000L, (long)new Money(1_250_000m));
    }

    [TestMethod]
    public void Amount_with_a_fraction_cannot_be_converted_to_long()
    {
        Assert.ThrowsException<InvalidOperationException>(() => (long)new Money(10.5m));
    }

    [TestMethod]
    [DataRow("fa-IR")]
    [DataRow("de-DE")]
    [DataRow("en-US")]
    public void Parse_uses_the_invariant_culture(string culture)
    {
        var current = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);

            Assert.AreEqual(new Money(1000.5m), Money.Parse("1000.5"));
            Assert.IsTrue(Money.TryParse("1250000", out var money));
            Assert.AreEqual(1_250_000m, money.Value);
            Assert.IsFalse(Money.TryParse("abc", out _));
            Assert.ThrowsException<FormatException>(() => Money.Parse("abc"));
        }
        finally
        {
            CultureInfo.CurrentCulture = current;
        }
    }
}
