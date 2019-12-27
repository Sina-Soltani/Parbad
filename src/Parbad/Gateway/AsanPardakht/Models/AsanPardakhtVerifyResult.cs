// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Internal;

namespace Parbad.Gateway.AsanPardakht.Models
{
    internal class AsanPardakhtVerifyResult
    {
        public bool IsSucceed { get; internal set; }
        public PaymentVerifyResult Result { get; internal set; }
    }
}
