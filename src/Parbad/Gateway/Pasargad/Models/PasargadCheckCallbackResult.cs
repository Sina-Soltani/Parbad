// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Internal;

namespace Parbad.Gateway.Pasargad.Models
{
    internal class PasargadCheckCallbackResult
    {
        public bool IsSucceed { get; set; }

        public PaymentVerifyResult Result { get; set; }
    }
}
