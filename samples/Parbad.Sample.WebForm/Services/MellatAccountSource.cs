using System.Threading.Tasks;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.Mellat;

namespace Parbad.Sample.WebForm.Services
{
    public class MellatAccountSource : IGatewayAccountSource<MellatGatewayAccount>
    {
        public Task AddAccountsAsync(IGatewayAccountCollection<MellatGatewayAccount> accounts)
        {
            accounts.Add(new MellatGatewayAccount
            {
                TerminalId = 123,
                UserName = "abc",
                UserPassword = "xyz"
            });

            return Task.CompletedTask;
        }
    }
}
