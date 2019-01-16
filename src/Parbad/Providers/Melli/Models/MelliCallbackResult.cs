using Parbad.Core;

namespace Parbad.Providers.Melli.Models
{
    internal class MelliCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string Token { get; set; }

        public object JsonDataToVerify { get; set; }

        public VerifyResult Result { get; set; }
    }
}
