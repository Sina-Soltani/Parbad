using System.Threading.Tasks;
using Parbad.GatewayBuilders;
using Parbad.GatewayProviders.Mellat;

namespace Parbad.Sample.Mvc.Services
{
    public class MellatAccountSource : IGatewayAccountSource<MellatGatewayAccount>
    {
        private readonly IMySettingsService _settingsService;

        public MellatAccountSource(IMySettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public Task AddAccountsAsync(IGatewayAccountCollection<MellatGatewayAccount> accounts)
        {
            var settings = _settingsService.GetSettings();

            accounts.Add(settings);

            return Task.CompletedTask;
        }
    }
}
