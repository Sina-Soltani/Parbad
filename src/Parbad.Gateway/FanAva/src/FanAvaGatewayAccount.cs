using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parbad.Abstraction;

namespace Parbad.Gateway.FanAva
{
    public class FanAvaGatewayAccount: GatewayAccount
    {
        public string UserId { get; set; }
        public string Password { get; set; }
    }
}
