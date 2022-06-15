// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.ZarinPal.Internal
{
    internal static class ZarinPalCodeTranslator
    {
        public static string Translate(int status, MessagesOptions options)
        {
            return status switch
            {
                100 => "عملیات موفق",
                101 => "تراکنش وریفای شده",
                -9 => "خطای اعتبار سنجی",
                -10 => "ای پی و يا مرچنت كد پذيرنده صحيح نيست",
                -11 => "مرچنت کد فعال نیست لطفا با تیم پشتیبانی ما تماس بگیرید",
                -12 => "تلاش بیش از حد در یک بازه زمانی کوتاه.",
                -15 => "ترمینال شما به حالت تعلیق در آمده با تیم پشتیبانی تماس بگیرید",
                -16 => "سطح تاييد پذيرنده پايين تر از سطح نقره اي است.",
                -30 => "اجازه دسترسی به تسویه اشتراکی شناور ندارید",
                -31 => "حساب بانکی تسویه را به پنل اضافه کنید مقادیر وارد شده واسه تسهیم درست نیست",
                -32 => "Wages is not valid, Total wages(floating) has been overload max amount.",
                -33 => "درصد های وارد شده درست نیست",
                -34 => "مبلغ از کل تراکنش بیشتر است",
                -35 => "تعداد افراد دریافت کننده تسهیم بیش از حد مجاز است",
                -40 => "Invalid extra params, expire_in is not valid.",
                -50 => "مبلغ پرداخت شده با مقدار مبلغ در وریفای متفاوت است",
                -51 => "پرداخت ناموفق",
                -52 => "خطای غیر منتظره با پشتیبانی تماس بگیرید",
                -53 => "اتوریتی برای این مرچنت کد نیست",
                -54 => "اتوریتی نامعتبر است",
                _ => $"{options.UnexpectedErrorText} Response: {status}"
            };
        }
    }
}
