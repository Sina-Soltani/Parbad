using Parbad.Core;

namespace Parbad.Providers.Parsian.Models
{
    internal class ParsianCallbackResult
    {
        public bool IsSucceed { get; set; }

        /// <summary>
        /// Equals to ReferenceID in Parbad system.
        /// </summary>
        public string Authority { get; set; }

        public VerifyResult Result { get; set; }
    }
}