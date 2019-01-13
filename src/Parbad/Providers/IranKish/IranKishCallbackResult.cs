using Parbad.Core;

namespace Parbad.Providers.IranKish
{
    internal class IranKishCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string Token { get; set; }

        /// <summary>
        /// Equals to ReferenceID in Parbad system.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Equals to TransactionID in Parbad system.
        /// </summary>
        public string ReferenceId { get; set; }

        public VerifyResult Result { get; set; }
    }
}
