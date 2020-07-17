// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.Sepehr.Internal
{
    internal static class SepehrGatewayResultTranslator
    {
        public static string Translate(string result, MessagesOptions options)
        {
            switch (result)
            {
                case "-1":
                    return "تراکنش پیدا نشد";

                case "-2":
                    return "در زمان دریافت توکن، به دلیل عدم وجود آی پی و یا به دلیل بسته بودن خروجی پورت ۸۰۸۱ از سمت هاست، این خطا ایجاد میگردد. تراکنش قبلا Reverse شده است";

                case "-3":
                    return "Total Error خطای عمومی";

                case "-4":
                    return "امکان انجام درخواست برای این تراکنش وجود ندارد.";

                case "-5":
                    return "آدرس IP نامعتبر می‌باشد.";

                case "-6":
                    return "عدم فعال بودن سرویس برگشت تراکنش برای پذیرنده.";

                default:
                    return options.UnexpectedErrorText;
            }
        }
    }
}