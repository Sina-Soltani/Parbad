// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Internal
{
    public class PaymentVerifyResult : PaymentResult, IPaymentVerifyResult
    {
        public string TransactionCode { get; set; }

        public static PaymentVerifyResult Succeed(string transactionCode, string message)
        {
            return new PaymentVerifyResult
            {
                IsSucceed = true,
                TransactionCode = transactionCode,
                Message = message
            };
        }

        public static PaymentVerifyResult Failed(string message)
        {
            return new PaymentVerifyResult
            {
                IsSucceed = false,
                Message = message
            };
        }
    }
}
