// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Internal
{
    public class PaymentRefundResult : PaymentResult, IPaymentRefundResult
    {
        public static PaymentRefundResult Succeed(string message = null)
        {
            return new PaymentRefundResult
            {
                IsSucceed = true,
                Message = message ?? string.Empty
            };
        }

        public static PaymentRefundResult Failed(string message)
        {
            return new PaymentRefundResult
            {
                IsSucceed = false,
                Message = message ?? string.Empty
            };
        }
    }
}
