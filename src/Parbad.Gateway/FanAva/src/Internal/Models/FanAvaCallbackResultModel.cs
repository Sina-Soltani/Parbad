using System.Collections.Generic;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaCallbackResultModel
    {
        public bool IsSucceed { get; set; }
        public string InvoiceNumber { get; set; }
        public string Message { get; set; }
        public FanAvaCheckRequestModel CallbackCheckData { get; set; }
    }
}