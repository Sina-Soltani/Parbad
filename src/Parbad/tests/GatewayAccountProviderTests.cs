using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Internal;
using System.Threading.Tasks;

namespace Parbad.Tests
{
    [TestClass]
    public class GatewayAccountProviderTests
    {
        [TestMethod]
        public async Task Must_Have_An_Account()
        {
            var source = new InMemoryGatewayAccountSource<TestableGatewayAccount>(new[]
            {
                new TestableGatewayAccount()
            });

            var provider = new GatewayAccountProvider<TestableGatewayAccount>(new[] { source });

            var accounts = await provider.LoadAccountsAsync();

            Assert.IsNotNull(accounts);
            Assert.IsTrue(accounts.Count == 1);
        }
    }
}
