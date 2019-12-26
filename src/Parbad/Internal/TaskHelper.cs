// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Parbad.Internal
{
    public static class TaskHelper
    {
        public static Task<IPaymentRequestResult> ToInterfaceAsync(this PaymentRequestResult requestResult)
        {
            if (requestResult == null) throw new ArgumentNullException(nameof(requestResult));

            return Task.FromResult<IPaymentRequestResult>(requestResult);
        }

        public static Task<IPaymentVerifyResult> ToInterfaceAsync(this PaymentVerifyResult verifyResult)
        {
            if (verifyResult == null) throw new ArgumentNullException(nameof(verifyResult));

            return Task.FromResult<IPaymentVerifyResult>(verifyResult);
        }

        public static Task<IPaymentRefundResult> ToInterfaceAsync(this PaymentRefundResult refundResult)
        {
            if (refundResult == null) throw new ArgumentNullException(nameof(refundResult));

            return Task.FromResult<IPaymentRefundResult>(refundResult);
        }
    }
}
