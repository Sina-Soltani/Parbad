using System.ComponentModel.DataAnnotations;
using Parbad.Abstraction;

namespace Parbad.GatewayProviders.IranKish
{
    /// <summary>
    /// IranKish gateway account.
    /// </summary>
    public class IranKishGatewayAccount : GatewayAccount
    {
        [Required(ErrorMessage = "MerchantId is required.", AllowEmptyStrings = false)]
        public string MerchantId { get; set; }
    }
}
