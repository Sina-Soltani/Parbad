using System.ComponentModel.DataAnnotations;
using Parbad.Abstraction;

namespace Parbad.GatewayProviders.Melli
{
    /// <summary>
    /// Melli Gateway options class.
    /// </summary>
    public class MelliGatewayAccount : GatewayAccount
    {
        [Required(ErrorMessage = "TerminalId is required.", AllowEmptyStrings = false)]
        public string TerminalId { get; set; }

        [Required(ErrorMessage = "MerchantId is required.", AllowEmptyStrings = false)]
        public string MerchantId { get; set; }

        [Required(ErrorMessage = "TerminalKey is required.", AllowEmptyStrings = false)]
        public string TerminalKey { get; set; }
    }
}
