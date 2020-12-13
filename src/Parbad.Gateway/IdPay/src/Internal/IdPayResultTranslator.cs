// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.IdPay.Internal
{
    internal static class IdPayResultTranslator
    {
        public static string TranslateStatus(string status, MessagesOptions options)
        {
            if (!int.TryParse(status, out var integerStatus))
            {
                return $"Cannot parse the Status value to integer. Status: {status}";
            }

            return integerStatus switch
            {
                1 => "پرداخت انجام نشده است",
                2 => "پرداخت ناموفق بوده است",
                3 => "خطا رخ داده است",
                4 => "بلوکه شده",
                5 => "برگشت به پرداخت کننده",
                6 => "برگشت خورده سیستمی",
                10 => "در انتظار تایید پرداخت",
                100 => "پرداخت تایید شده است",
                101 => "پرداخت قبلا تایید شده است",
                200 => "به دریافت کننده واریز شد",
                _ => $"{options.UnexpectedErrorText} Response: {status}"
            };
        }
    }
}
