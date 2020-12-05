using NUnit.Framework;
using Parbad.Internal;
using System.Threading.Tasks;

namespace Parbad.Tests
{
    public class GatewayAccountProviderTests
    {
        [Test]
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
