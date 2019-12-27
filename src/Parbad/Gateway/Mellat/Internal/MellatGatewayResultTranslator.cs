// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.Mellat.Internal
{
    internal static class MellatGatewayResultTranslator
    {
        public static string Translate(string result, MessagesOptions options)
        {
            switch (result)
            {
                case "0":
                    return "تراكنش با موفقيت انجام شد";
                case "11":
                    return "شماره كارت نامعتبر است";
                case "12":
                    return "موجودي كافي نيست";
                case "13":
                    return "رمز نادرست است";
                case "14":
                    return "تعداد دفعات وارد كردن رمز بيش از حد مجاز است";
                case "15":
                    return "كارت نامعتبر است";
                case "16":
                    return "دفعات برداشت وجه بيش از حد مجاز است";
                case "17":
                    return "كاربر از انجام تراكنش منصرف شده است";
                case "18":
                    return "تاريخ انقضاي كارت گذشته است";
                case "19":
                    return "مبلغ برداشت وجه بيش از حد مجاز است";
                case "111":
                    return "صادر كننده كارت نامعتبر است";
                case "112":
                    return "خطاي سوييچ صادر كننده كارت";
                case "113":
                    return "پاسخي از صادر كننده كارت دريافت نشد";
                case "114":
                    return "دارنده كارت مجاز به انجام اين تراكنش نيست";
                case "21":
                    return "پذيرنده نامعتبر است";
                case "23":
                    return "خطاي امنيتي رخ داده است";
                case "24":
                    return "اطلاعات كاربري پذيرنده نامعتبر است";
                case "25":
                    return "مبلغ نامعتبر است";
                case "31":
                    return "پاسخ نامعتبر است";
                case "32":
                    return "فرمت اطلاعات وارد شده صحيح نمي باشد";
                case "33":
                    return "حساب نامعتبر است";
                case "34":
                    return "خطاي سيستمي";
                case "35":
                    return "تاريخ نامعتبر است";
                case "41":
                    return "شماره درخواست تكراري است";
                case "42":
                    return "تراکنش Sale یافت نشد";
                case "43":
                    return "قبلا درخواست Verify داده شده است";
                case "44":
                    return "درخواست Verify یافت نشد";
                case "45":
                    return "تراکنش Settle شده است";
                case "46":
                    return "تراکنش Settle نشده است";
                case "47":
                    return "تراکنش Settle یافت نشد";
                case "48":
                    return "تراکنش Reverse شده است";
                case "49":
                    return "تراکنش Refund یافت نشد";
                case "412":
                    return "شناسه قبض نادرست است";
                case "413":
                    return "شناسه پرداخت نادرست است";
                case "414":
                    return "سازمان صادر كننده قبض نامعتبر است";
                case "415":
                    return "زمان جلسه كاري به پايان رسيده است";
                case "416":
                    return "خطا در ثبت اطلاعات";
                case "417":
                    return "شناسه پرداخت كننده نامعتبر است";
                case "418":
                    return "اشكال در تعريف اطلاعات مشتري";
                case "419":
                    return "تعداد دفعات ورود اطلاعات از حد مجاز گذشته است";
                case "421":
                    return "IP نامعتبر است";
                case "51":
                    return "تراكنش تكراري است";
                case "54":
                    return "تراكنش مرجع موجود نيست";
                case "55":
                    return "تراكنش نامعتبر است";
                case "61":
                    return "خطا در واريز";
                default:
                    return options.UnexpectedErrorText;
            }
        }
    }
}
