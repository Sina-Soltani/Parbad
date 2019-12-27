// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.Saman.Internal.ResultTranslators
{
    internal static class SamanStateTranslator
    {
        public static string Translate(string result, MessagesOptions options)
        {
            switch (result)
            {
                case "":
                case "Canceled By User":
                    return "تراكنش توسط خريدار كنسل شد";

                case "Invalid Amount":
                    return "مبلغ سند برگش ت ي، از مبلغ تراکنش اص ل ي بیشتراست.";

                case "Invalid Transaction":
                    return "درخواست برگشت یک تراکنش رسیده است، در حالي که تراکنش اصلي پیدا نمي شود.";

                case "Invalid Card Number":
                    return "شماره کارت اشتباه است.";

                case "No Such Issuer":
                    return "چنین صادر کننده کارتي وجود ندارد.";

                case "Expired Card Pick Up":
                    return "از تاریخ انقضاي کارت گذشته اس ت و کارت دیگر معتبر نیست.";

                case "Allowable PIN Tries Exceeded Pick Up":
                    return "۳ مرتبه اشتباه وارد شده است (PIN) رمز کارت در نتیجه کارت غیر فعال خواهد شد.";

                case "Incorrect PIN":
                    return "را اشتباه وارد کرده (PIN) خریدار رمز کارت";

                case "Exceeds Withdrawal Amount Limit":
                    return "مبلغ بیش از سقف برداشت مي باشد.";

                case "Transaction Cannot Be Completed":
                    return "PIN شده است ( شماره Authorize تراکنش درست هستند) ولي امکان سند خوردن PAN ووجود ندارد.";

                case "Response Received Too Late":
                    return "خورده Timeout تراکنش در شبکه بانکي است.";

                case "Suspected Fraud Pick Up":
                    return "را ExpDate و یا فیلد CVV خریدار یا فیلد 2 اشتباه زده است. (یا اصلا وارد نکرده است)";

                case "No Sufficient Funds":
                    return "موجودي به اندازي کافي در حساب وجود ندارد.";

                case "Issuer Down Slm":
                    return "سیستم کارت بانک صادر کننده در وضعیت عملیاتي نیست.";

                default:
                    return options.UnexpectedErrorText;
            }
        }
    }
}
