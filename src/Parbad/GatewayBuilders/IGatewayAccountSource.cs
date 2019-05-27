using System.Threading.Tasks;
using Parbad.Abstraction;

namespace Parbad.GatewayBuilders
{
    public interface IGatewayAccountSource<TAccount> where TAccount : GatewayAccount
    {
        Task AddAccountsAsync(IGatewayAccountCollection<TAccount> accounts);
    }
}
