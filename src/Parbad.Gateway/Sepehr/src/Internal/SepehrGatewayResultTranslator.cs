// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.Sepehr.Internal
{
    internal static class SepehrGatewayResultTranslator
    {
        public static string Translate(string result, MessagesOptions options)
        {
            return result switch
            {
                "-1" => "تراکنش پیدا نشد",
                "-2" =>
                "در زمان دریافت توکن، به دلیل عدم وجود آی پی و یا به دلیل بسته بودن خروجی پورت ۸۰۸۱ از سمت هاست، این خطا ایجاد میگردد. تراکنش قبلا Reverse شده است",
                "-3" => "Total Error خطای عمومی",
                "-4" => "امکان انجام درخواست برای این تراکنش وجود ندارد.",
                "-5" => "آدرس IP نامعتبر می‌باشد.",
                "-6" => "عدم فعال بودن سرویس برگشت تراکنش برای پذیرنده.",
                _ => $"{options.UnexpectedErrorText} Response: {result}"
            };
        }
    }
}
