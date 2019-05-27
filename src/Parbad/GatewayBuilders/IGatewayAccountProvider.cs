using System.Threading.Tasks;
using Parbad.Abstraction;

namespace Parbad.GatewayBuilders
{
    public interface IGatewayAccountProvider<TAccount> where TAccount : GatewayAccount
    {
        Task<IGatewayAccountCollection<TAccount>> LoadAccountsAsync();
    }
}
