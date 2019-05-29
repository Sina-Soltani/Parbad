using System.ComponentModel.DataAnnotations;
using Parbad.Abstraction;

namespace Parbad.GatewayProviders.Pasargad
{
    public class PasargadGatewayAccount : GatewayAccount
    {
        [Required(ErrorMessage = "MerchantCode is required.", AllowEmptyStrings = false)]
        public string MerchantCode { get; set; }

        [Required(ErrorMessage = "TerminalCode is required.", AllowEmptyStrings = false)]
        public string TerminalCode { get; set; }

        [Required(ErrorMessage = "PrivateKey is required.", AllowEmptyStrings = false)]
        [PasargadPrivateKeyValidation(ErrorMessage = "Pasargad Private Key is not a valid XML. Please check your Private Key again.")]
        public string PrivateKey { get; set; }
    }
}
