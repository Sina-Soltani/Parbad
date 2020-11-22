using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.GatewayBuilders;
using Parbad.Internal;

namespace Parbad.Tests
{
    [TestClass]
    public class InMemoryAccountSourceTests
    {
        [TestMethod]
        public async Task Must_Have_An_Account()
        {
            IGatewayAccountSource<TestableGatewayAccount> source = new InMemoryGatewayAccountSource<TestableGatewayAccount>(new[]
            {
                new TestableGatewayAccount()
            });

            var accounts = new GatewayAccountCollection<TestableGatewayAccount>();

            await source.AddAccountsAsync(accounts);

            Assert.IsTrue(accounts.Count == 1);
        }

        [TestMethod]
        public async Task Must_Have_The_Expected_Account()
        {
            const int expectedId = 1;

            IGatewayAccountSource<TestableGatewayAccount> source = new InMemoryGatewayAccountSource<TestableGatewayAccount>(new[]
            {
                new TestableGatewayAccount{ Id = expectedId }
            });

            var accounts = new GatewayAccountCollection<TestableGatewayAccount>();

            await source.AddAccountsAsync(accounts);

            Assert.AreEqual(expectedId, accounts.First().Id);
        }
    }
}
