// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.Saman.Internal.ResultTranslators
{
    internal static class SamanStateTranslator
    {
        public static string Translate(string result, MessagesOptions options)
        {
            return result switch
            {
                "" => "تراكنش توسط خريدار كنسل شد",
                "Canceled By User" => "تراكنش توسط خريدار كنسل شد",
                "Invalid Amount" => "مبلغ سند برگش ت ي، از مبلغ تراکنش اص ل ي بیشتراست.",
                "Invalid Transaction" => "درخواست برگشت یک تراکنش رسیده است، در حالي که تراکنش اصلي پیدا نمي شود.",
                "Invalid Card Number" => "شماره کارت اشتباه است.",
                "No Such Issuer" => "چنین صادر کننده کارتي وجود ندارد.",
                "Expired Card Pick Up" => "از تاریخ انقضاي کارت گذشته اس ت و کارت دیگر معتبر نیست.",
                "Allowable PIN Tries Exceeded Pick Up" =>
                "۳ مرتبه اشتباه وارد شده است (PIN) رمز کارت در نتیجه کارت غیر فعال خواهد شد.",
                "Incorrect PIN" => "را اشتباه وارد کرده (PIN) خریدار رمز کارت",
                "Exceeds Withdrawal Amount Limit" => "مبلغ بیش از سقف برداشت مي باشد.",
                "Transaction Cannot Be Completed" =>
                "PIN شده است ( شماره Authorize تراکنش درست هستند) ولي امکان سند خوردن PAN ووجود ندارد.",
                "Response Received Too Late" => "خورده Timeout تراکنش در شبکه بانکي است.",
                "Suspected Fraud Pick Up" =>
                "را ExpDate و یا فیلد CVV خریدار یا فیلد 2 اشتباه زده است. (یا اصلا وارد نکرده است)",
                "No Sufficient Funds" => "موجودي به اندازي کافي در حساب وجود ندارد.",
                "Issuer Down Slm" => "سیستم کارت بانک صادر کننده در وضعیت عملیاتي نیست.",
                _ => $"{options.UnexpectedErrorText} Response: {result}"
            };
        }
    }
}
