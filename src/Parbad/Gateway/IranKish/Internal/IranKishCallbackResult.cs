// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.IranKish.Internal
{
    internal class IranKishCallbackResult
    {
        public bool IsSucceed { get; set; }

        public string Token { get; set; }

        /// <summary>
        /// Equals to TrackingNumber in Parbad system.
        /// </summary>
        public long InvoiceNumber { get; set; }

        /// <summary>
        /// Equals to TransactionCode in Parbad system.
        /// </summary>
        public string ReferenceId { get; set; }

        public IPaymentVerifyResult Result { get; set; }
    }
}
