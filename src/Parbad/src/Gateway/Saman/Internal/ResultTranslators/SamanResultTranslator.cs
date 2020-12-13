// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.Saman.Internal.ResultTranslators
{
    internal static class SamanResultTranslator
    {
        public static string Translate(long result, MessagesOptions options)
        {
            if (result >= 0)
            {
                return "تراکنش با موفقیت انجام گردید";
            }

            return result switch
            {
                -1 => "خطای داخلی شبکه مالی",
                -2 => "سپرده ها برابر نیستند",
                -3 => "ورودی ها حاوی کاراکترهای غیرمجاز هستند",
                -4 => "کد پذیرنده و یا کلمه عبور آن اشتباه است",
                -5 => "Database Exception",
                -6 => "سند قبلا برگشت کامل یافته است",
                -7 => "رسید دیجیتالی تهی است",
                -8 => "طول ورودی ها بیشتر از حد مجاز است",
                -9 => "وجود کاراکترهای غیرمجاز در مبلغ برگشتی",
                -10 => "رسید دیجیتالی حاوی کاراکترهای غیرمجاز است",
                -11 => "طول ورودی ها کمتر از حد مجاز است",
                -12 => "مبلغ برگشتی منفی است",
                -13 => "مبلغ برگشتی برای برگشت جزئی بیش از مبلغ برگشت نخورده رسیده دیجیتالی است",
                -14 => "چنین تراکنشی تعریف نشده است",
                -15 => "مبلغ برگشتی بصورت اعشاری داده شده است",
                -16 => "خطای داخلی سیستم",
                -17 => "برگشت زدن جزئی تراکنشی که با کارتی غیر از بانک سامان انجام شده است",
                -18 => "IP Address پذیرنده نامعتبر است",
                _ => $"{options.UnexpectedErrorText} Response: {result}"
            };
        }
    }
}
