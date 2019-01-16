using System;

namespace Parbad.Providers.Melli.Models
{
    [Serializable]
    internal class MelliApiVerifyResult
    {
        public int ResCode { get; set; }

        public long Amount { get; set; }

        public string Description { get; set; }

        public string RetrivalRefNo { get; set; }

        public string SystemTraceNo { get; set; }

        public long OrderId { get; set; }
    }
}