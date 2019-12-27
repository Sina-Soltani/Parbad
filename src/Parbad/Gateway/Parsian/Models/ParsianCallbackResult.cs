// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Internal;

namespace Parbad.Gateway.Parsian.Models
{
    internal class ParsianCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string Token { get; set; }

        /// <summary>
        /// Equals to TransactionCode in Parbad system.
        /// </summary>
        public string RRN { get; set; }

        public PaymentVerifyResult Result { get; set; }
    }
}
