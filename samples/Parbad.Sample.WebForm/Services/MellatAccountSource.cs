using System.Threading.Tasks;
using Parbad.Gateway.Mellat;
using Parbad.GatewayBuilders;

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
