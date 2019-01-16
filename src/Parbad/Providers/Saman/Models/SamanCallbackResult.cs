using Parbad.Core;

namespace Parbad.Providers.Saman.Models
{
    internal class SamanCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string ReferenceId { get; set; }

        public string TransactionId { get; set; }

        public VerifyResult Result { get; set; }
    }
}
