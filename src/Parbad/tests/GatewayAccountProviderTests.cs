using Parbad.Internal;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Parbad.Tests;

[TestFixture]
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

        Assert.That(accounts, Is.Not.Null);
        Assert.That(accounts.Count == 1, Is.True);
    }
}