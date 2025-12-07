// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.Tara
{
    internal static class TaraGatewayResultTranslator
    {
        public static string Translate(string result, MessagesOptions messagesOptions)
        {
            return result switch
            {
                "0" => "موفق",
                "1" => "درخواست از IP غیر مجاز",
                "2" => "نام کاربری یا رمز عبور نامعتبر است",
                "3" => "کاربر دسترسی ندارد",
                "4" => "پذیرنده یافت نشد",
                "5" => "هدایت به صفحه پرداخت",
                "6" => "تراکنش یافت نشد",
                "7" => "شماره سرویس نامعتبر است",
                "8" => "توکن تکراری است",
                "9" => "مبالغ یکسان نیست",
                "10" => "کانال یافت نشد",
                "11" => "مبلغ بیشتر از حد مجاز",
                "12" => "مبلغ کمتر از حد مجاز",
                "13" => "مبلغ نمی تواند خالی باشد",
                "14" => "IP نمی تواد خالی باشد",
                "15" => "مبلغ نامعتبر می باشد",
                "16" => "لیست مبالغ سرویس خالی میباشد",
                "17" => "شناسه سرویس نامعتبر",
                "18" => "فرمت آدرس برگشتی صحیح نمی‌باشد",
                "19" => "خطای عمومی",
                "20" => "توکن یافت نشد",
                "21" => "شماره پیگیری به پذیرنده تعلق ندارد",
                "22" => "خطای عمومی",
                "23" => "تراکنش اصلی موفق نبوده است",
                _ => messagesOptions.PaymentFailed
            };
        }
    }
}

