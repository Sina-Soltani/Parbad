using System.Collections.Generic;

namespace Parbad.Gateway.FanAva.Internal.Models
{

    internal class FanAvaCheckRequestModel
    {
        public FanAvaRequestModel.WSContextModel WSContext { get; set; }
        public string Token { get; set; }
    }

}