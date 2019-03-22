using Parbad.GatewayProviders.Mellat;
using Parbad.Options;

namespace Parbad.Sample.Mvc.Services
{
    public class MellatOptionsProvider : IParbadOptionsProvider<MellatGatewayOptions>
    {
        private readonly IMySettingsService _settingsService;

        public MellatOptionsProvider(IMySettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public void Provide(MellatGatewayOptions options)
        {
            var settings = _settingsService.GetSettings();

            options.TerminalId = settings.TerminalId;
            options.UserName = settings.UserName;
            options.UserPassword = settings.UserPassword;
        }
    }
}
