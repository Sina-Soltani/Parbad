using Parbad.Core;

namespace Parbad.Providers.Pasargad.Models
{
    internal class PasargadCheckCallbackResult
    {
        public bool IsSucceed { get; set; }

        public VerifyResult Result { get; set; }
    }
}
