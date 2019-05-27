using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Parbad.Abstraction;
using Parbad.GatewayBuilders;

namespace Parbad.Internal
{
    public class MsConfigurationGatewayAccountSource<TAccount> : IGatewayAccountSource<TAccount>
        where TAccount : GatewayAccount, new()
    {
        public MsConfigurationGatewayAccountSource(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public Task AddAccountsAsync(IGatewayAccountCollection<TAccount> accounts)
        {
            var newAccount = new TAccount();

            Configuration.Bind(newAccount);

            accounts.Add(newAccount);

            return Task.CompletedTask;
        }
    }
}
