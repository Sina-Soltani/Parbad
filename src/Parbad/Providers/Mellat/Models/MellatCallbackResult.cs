using Parbad.Core;

namespace Parbad.Providers.Mellat.Models
{
    internal class MellatCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string RefId { get; set; }

        public string SaleReferenceId { get; set; }

        public VerifyResult Result { get; set; }
    }
}