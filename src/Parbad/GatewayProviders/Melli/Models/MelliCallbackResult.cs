// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.GatewayProviders.Melli.Models
{
    internal class MelliCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string Token { get; set; }

        public object JsonDataToVerify { get; set; }

        public IPaymentVerifyResult Result { get; set; }
    }
}
