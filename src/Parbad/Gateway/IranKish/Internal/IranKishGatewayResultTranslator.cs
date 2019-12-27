// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.IranKish.Internal
{
    internal static class IranKishGatewayResultTranslator
    {
        public static string Translate(string result, MessagesOptions options)
        {
            switch (result)
            {
                case "100":
                    return "موفقیت تراکنش.";
                case "110":
                    return "انصراف دارنده کارت.";
                case "120":
                    return "موجودی حساب کافی نیست.";
                case "130":
                    return "اطلاعات کارت اشتباه است";
                case "131":
                    return "رمز کارت اشتباه است.";
                case "132":
                    return "کارت مسدود شده است.";
                case "133":
                    return "کارت منقضی شده است.";
                case "140":
                    return "زمان مورد نظر به پایان رسیده است.";
                case "150":
                    return "خطای داخلی بانک";
                case "160":
                    return "خطا در اطلاعات CVV2 یا ExpDate.";
                case "166":
                    return "بانک صادر کننده کارت شما مجوز انجام تراکنش را صادر نکرده است.";
                case "200":
                    return "مبلغ تراکنش بیشتر از سقف مجاز برای هر تراکنش می‌باشد.";
                case "201":
                    return "مبلغ تراکنش بیشتر از سقف مجاز در روز می‌باشد.";
                case "202":
                    return "مبلغ تراکنش بیشتر از سقف مجاز در ماه می‌باشد.";
                case "-20":
                    return "وجود کاراکتر‌های غیر مجاز در درخواست.";
                case "-30":
                    return "تراکنش قبلا برگشت خورده است.";
                case "-50":
                    return "طول رشته درخواست غیر مجاز است.";
                case "-51":
                    return "خطا در درخواست.";
                case "-80":
                    return "تراکنش مورد نظر یافت نشد.";
                case "-81":
                    return "خطای داخلی بانک.";
                case "-90":
                    return "تراکنش قبلا تایید شده است.";
                default:
                    return options.UnexpectedErrorText;
            }
        }
    }
}
