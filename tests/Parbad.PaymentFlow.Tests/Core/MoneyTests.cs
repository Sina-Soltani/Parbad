using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.PaymentFlow.Tests.Infrastructure;

namespace Parbad.PaymentFlow.Tests.Core;

/// <summary>
/// Amount comparisons that payment checks depend on. Issue ids refer to ANALYSIS-01-CORE-ARCHITECTURE.md.
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
    public Task Equal_amounts_are_not_less_than_each_other() =>
        KnownIssue.ReproducesAsync("C-4", () =>
        {
            Assert.IsFalse(new Money(5) < new Money(5), "Money(5) < Money(5) returned true.");
        });

    [TestMethod]
    public Task Amount_with_a_fraction_is_not_equal_to_its_truncated_value() =>
        KnownIssue.ReproducesAsync("C-5", () =>
        {
            Assert.IsFalse(new Money(10.5m).Equals(10L), "Money(10.5).Equals(10L) returned true.");
        });
}
