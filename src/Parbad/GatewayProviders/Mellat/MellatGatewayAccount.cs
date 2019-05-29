using System.ComponentModel.DataAnnotations;
using Parbad.Abstraction;

namespace Parbad.GatewayProviders.Mellat
{
    public class MellatGatewayAccount : GatewayAccount
    {
        public long TerminalId { get; set; }

        [Required(ErrorMessage = "User Name is required.", AllowEmptyStrings = false)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "User Password is required.", AllowEmptyStrings = false)]
        public string UserPassword { get; set; }

        /// <summary>
        /// The requests will be sent to the test terminal of Mellat Gateway.
        /// </summary>
        public bool IsTestTerminal { get; set; }
    }
}
