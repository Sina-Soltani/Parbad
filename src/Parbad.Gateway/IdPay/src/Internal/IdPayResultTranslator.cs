// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.IdPay.Internal
{
    internal static class IdPayResultTranslator
    {
        public static string TranslateStatus(string status, MessagesOptions messagesOptions)
        {
            if (!int.TryParse(status, out var integerStatus))
            {
                return $"Cannot parse the Status value to integer. Status: {status}";
            }

            switch (integerStatus)
            {
                case 1:
                    return "پرداخت انجام نشده است";
                case 2:
                    return "پرداخت ناموفق بوده است";
                case 3:
                    return "خطا رخ داده است";
                case 4:
                    return "بلوکه شده";
                case 5:
                    return "برگشت به پرداخت کننده";
                case 6:
                    return "برگشت خورده سیستمی";
                case 10:
                    return "در انتظار تایید پرداخت";
                case 100:
                    return "پرداخت تایید شده است";
                case 101:
                    return "پرداخت قبلا تایید شده است";
                case 200:
                    return "به دریافت کننده واریز شد";
                default:
                    return messagesOptions.UnexpectedErrorText;
            }
        }
    }
}
