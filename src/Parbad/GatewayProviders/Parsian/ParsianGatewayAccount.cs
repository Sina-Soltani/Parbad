using System.ComponentModel.DataAnnotations;
using Parbad.Abstraction;

namespace Parbad.GatewayProviders.Parsian
{
    public class ParsianGatewayAccount : GatewayAccount
    {
        [Required(ErrorMessage = "LoginAccount is required.", AllowEmptyStrings = false)]
        public string LoginAccount { get; set; }
    }
}
