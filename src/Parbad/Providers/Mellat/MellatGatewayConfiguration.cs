using System;
using Parbad.Utilities;

namespace Parbad.Providers.Mellat
{
    public class MellatGatewayConfiguration
    {
        public MellatGatewayConfiguration(long terminalId, string userName, string userPassword) : this(terminalId, userName, userPassword, false)
        {
        }

        public MellatGatewayConfiguration(long terminalId, string userName, string userPassword, bool isTestModeEnabled)
        {
            if (userName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(userName), $"The Gateway: \"Mellat\" configuration failed. \"{nameof(userName)}\" is null or empty");
            }

            if (userPassword.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(userPassword), $"The Gateway: \"Mellat\" configuration failed. \"{nameof(userPassword)}\" is null or empty");
            }

            TerminalId = terminalId;
            UserName = userName;
            UserPassword = userPassword;
            IsTestModeEnabled = isTestModeEnabled;
        }

        public long TerminalId { get; }

        public string UserName { get; }

        public string UserPassword { get; }

        /// <summary>
        /// Enables Mellat test gateway. Default value is false
        /// </summary>
        public bool IsTestModeEnabled { get; }
    }
}