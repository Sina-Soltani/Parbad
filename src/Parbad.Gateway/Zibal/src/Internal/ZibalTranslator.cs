// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Zibal.Internal
{
    internal static class ZibalTranslator
    {
        public static string? TranslateResult(int result)
        {
            return result switch
            {
                100 => "با موفقیت تایید شد.",
                102 => "merchant یافت نشد",
                103 => "merchant غیرفعال است",
                104 => "merchant نامعتبر است",
                105 => "amount بایستی بزرگتر از 1,000 ریال باشد.",
                106 => "callbackUrl نامعتبر می‌باشد. (شروع با http و یا https)",
                114 => "مبلغ تراکنش از سقف میزان تراکنش بیشتر است.",
                107 => "percentMode نامعتبر می‌باشد.",
                112 => "موجودی کیف‌پول اصلی شما جهت ثبت این سفارش کافی نمی‌باشد. (در صورتی که feeMode == 1 )",
                _ => null
            };
        }

        public static string? TranslateStatus(int status)
        {
            return status switch
            {
                -1 => "در انتظار پردخت",
                -2 => "خطای داخلی",
                1 => "پرداخت شده - تاییدشده",
                2 => "پرداخت شده - تاییدنشده",
                3 => "لغوشده توسط کاربر",
                4 => "‌شماره کارت نامعتبر می‌باشد.",
                5 => "‌موجودی حساب کافی نمی‌باشد.",
                6 => "رمز واردشده اشتباه می‌باشد.",
                7 => "‌تعداد درخواست‌ها بیش از حد مجاز می‌باشد.",
                8 => "‌تعداد پرداخت اینترنتی روزانه بیش از حد مجاز می‌باشد.",
                9 => "مبلغ پرداخت اینترنتی روزانه بیش از حد مجاز می‌باشد.",
                10 => "‌صادرکننده‌ی کارت نامعتبر می‌باشد.",
                11 => "‌خطای سوییچ",
                12 => "کارت قابل دسترسی نمی‌باشد.",
                _ => null
            };
        }
    }
}
