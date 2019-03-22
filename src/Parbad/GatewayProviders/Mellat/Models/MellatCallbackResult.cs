// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.GatewayProviders.Mellat.Models
{
    internal class MellatCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string RefId { get; set; }

        public string SaleReferenceId { get; set; }

        public IPaymentVerifyResult Result { get; set; }
    }
}
