using Parbad.GatewayProviders.Mellat;

namespace Parbad.Sample.Mvc.Services
{
    public interface IMySettingsService
    {
        MellatGatewayAccount GetSettings();
    }

    public class MySettingsService : IMySettingsService
    {
        public MellatGatewayAccount GetSettings()
        {
            // Just an example. You can get the settings from your database.

            return new MellatGatewayAccount
            {
                TerminalId = 123,
                UserName = "abc",
                UserPassword = "xyz"
            };
        }
    }
}
