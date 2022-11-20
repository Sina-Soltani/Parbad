using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parbad.Tests
{
    [TestClass]
    public class MoneyTests
    {
        private const decimal ExpectedAmount = 1000;

        [TestMethod]
        public void Amount_Must_Be_Equal_With_ExpectedAmount()
        {
            var instance1 = new Money(ExpectedAmount);
            var instance2 = Money.Parse(ExpectedAmount.ToString());
            Money.TryParse(ExpectedAmount.ToString(), out var instance3);

            Assert.AreEqual(ExpectedAmount, instance1.Value);
            Assert.AreEqual(ExpectedAmount, instance2.Value);
            Assert.AreEqual(ExpectedAmount, instance3.Value);
        }

        [TestMethod]
        public void ToString_Value_Must_Be_Equal_With_Amount()
        {
            var instance = new Money(ExpectedAmount);

            Assert.AreEqual(ExpectedAmount.ToString(), instance.ToString());
        }

        [TestMethod]
        public void Casted_Value_Must_Be_Equal_With_Amount()
        {
            var amount = (long)new Money(ExpectedAmount);

            Assert.AreEqual(ExpectedAmount, amount);
        }

        [TestMethod]
        public void Added_Amount_Must_Be_Equal_With_2000()
        {
            var instance = new Money(ExpectedAmount).AddAmount(ExpectedAmount);

            Assert.AreEqual(ExpectedAmount * 2, (decimal)instance);
        }

        [TestMethod]
        public void Compare_Method_Works()
        {
            var instance = new Money(ExpectedAmount);
            var instance2 = new Money(ExpectedAmount);

            Assert.AreEqual(0, instance.CompareTo(instance2));
        }

        [TestMethod]
        public void Equatable_Works()
        {
            var money = new Money(10);

            Assert.AreEqual(money, 10L);
            Assert.AreEqual(money, 10M);
            Assert.AreEqual(money, new Money(10));
        }
    }
}
