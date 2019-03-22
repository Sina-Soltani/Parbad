using Parbad.GatewayProviders.Mellat;

namespace Parbad.Sample.Mvc.Services
{
    public interface IMySettingsService
    {
        MellatGatewayOptions GetSettings();
    }

    public class MySettingsService : IMySettingsService
    {
        public MellatGatewayOptions GetSettings()
        {
            // Just an example. You can get the settings from your database.

            return new MellatGatewayOptions
            {
                TerminalId = 123,
                UserName = "abc",
                UserPassword = "xyz"
            };
        }
    }
}
