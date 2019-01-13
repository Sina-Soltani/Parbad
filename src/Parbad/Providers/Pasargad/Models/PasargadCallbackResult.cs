using Parbad.Core;

namespace Parbad.Providers.Pasargad.Models
{
    internal class PasargadCallbackResult
    {
        public bool IsSucceed { get; set; }

        /// <summary>
        /// Equals to ReferenceID in Parbad system.
        /// </summary>
        public string InvoiceNumber { get; set; }

        public string InvoiceDate { get; set; }

        public string TransactionId { get; set; }

        public string CallbackCheckData { get; set; }

        public VerifyResult Result { get; set; }
    }
}
