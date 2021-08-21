using Newtonsoft.Json;
using System;

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaVerifyResultModel
    {
        public bool IsSucceed => !string.IsNullOrWhiteSpace(Result) &&
                                 Result.Equals("erSucceed", StringComparison.OrdinalIgnoreCase);

        public string Result { get; set; }

        public string Amount { get; set; }

        [JsonProperty("RefNum")]
        public string InvoiceNumber { get; set; }
    }
}
