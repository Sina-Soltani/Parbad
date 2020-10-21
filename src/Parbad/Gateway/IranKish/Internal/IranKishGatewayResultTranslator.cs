// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.IranKish.Internal
{
    internal static class IranKishGatewayResultTranslator
    {
        public static string Translate(string result, MessagesOptions options)
        {
            return result switch
            {
                "100" => "موفقیت تراکنش.",
                "110" => "انصراف دارنده کارت.",
                "120" => "موجودی حساب کافی نیست.",
                "130" => "اطلاعات کارت اشتباه است",
                "131" => "رمز کارت اشتباه است.",
                "132" => "کارت مسدود شده است.",
                "133" => "کارت منقضی شده است.",
                "140" => "زمان مورد نظر به پایان رسیده است.",
                "150" => "خطای داخلی بانک",
                "160" => "خطا در اطلاعات CVV2 یا ExpDate.",
                "166" => "بانک صادر کننده کارت شما مجوز انجام تراکنش را صادر نکرده است.",
                "200" => "مبلغ تراکنش بیشتر از سقف مجاز برای هر تراکنش می‌باشد.",
                "201" => "مبلغ تراکنش بیشتر از سقف مجاز در روز می‌باشد.",
                "202" => "مبلغ تراکنش بیشتر از سقف مجاز در ماه می‌باشد.",
                "-20" => "وجود کاراکتر‌های غیر مجاز در درخواست.",
                "-30" => "تراکنش قبلا برگشت خورده است.",
                "-50" => "طول رشته درخواست غیر مجاز است.",
                "-51" => "خطا در درخواست.",
                "-80" => "تراکنش مورد نظر یافت نشد.",
                "-81" => "خطای داخلی بانک.",
                "-90" => "تراکنش قبلا تایید شده است.",
                _ => options.UnexpectedErrorText
            };
        }
    }
}
