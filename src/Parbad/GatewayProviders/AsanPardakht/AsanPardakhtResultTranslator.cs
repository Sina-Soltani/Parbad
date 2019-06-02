// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Parbad.Options;

namespace Parbad.GatewayProviders.AsanPardakht
{
    internal static class AsanPardakhtResultTranslator
    {
        public static string TranslateRequest(string result, MessagesOptions options) =>
            Translate(result, RequestDictionary, options);

        public static string TranslateVerification(string result, MessagesOptions options) =>
            Translate(result, VerificationDictionary, options);

        public static string TranslateReconcilation(string result, MessagesOptions options) =>
            Translate(result, ReconcilationDictionary, options);

        public static string TranslateReversal(string result, MessagesOptions options) =>
            Translate(result, ReversalDictionary, options);

        private static string Translate(string result, IDictionary<int, string> dictionary, MessagesOptions options)
        {
            var code = int.Parse(result);

            if (dictionary.ContainsKey(code))
            {
                return dictionary[code];
            }

            return options.UnexpectedErrorText;
        }

        private static IDictionary<int, string> RequestDictionary => new Dictionary<int, string>
        {
            {301, "پيكربندي پذيرنده اينترنتي نامعتبر است"},
            {302, "كليدهاي رمزنگاري نامعتبر هستند"},
            {303, "رمزنگاري نامعتبر است"},
            {304, "تعداد عناصر درخواست نامعتبر است"},
            {305, "نام كاربري يا رمز عبور پذيرنده نامعتبر است"},
            {306, "با آسان پرداخت تماس بگيريد"},
            {307, "سرور پذيرنده نامعتبر است"},
            {308, "شناسه فاكتور مي بايست صرفا عدد باشد"},
            {309, "مبلغ فاكتور نادرست ارسال شده است"},
            {310, "طول فيلد تاريخ و زمان نامعتبر است"},
            {311, "فرمت تاريخ و زمان ارسالي پذيرنده نامعتبر است"},
            {312, "نوع سرويس نامعتبر است"},
            {313, "شناسه پرداخت كننده نامعتبر است"},
            {315, "فرمت توصيف شيوه تسهيم شبا نامعتبر است"},
            {316, "شيوه تقسيم وجوه با مبلغ كل تراكنش همخواني ندارد"},
            {317, "شبا متعلق به پذيرنده نيست"},
            {318, "هيچ شبايي براي پذيرنده موجود نيست"},
            {319, "خطاي داخلي. دوباره درخواست ارسال شود"},
            {320, "شباي تكراري در رشته درخواست ارسال شده است"},
            {-100, "تاريخ ارسالي محلي پذيرنده نامعتبر است"},
            {-103, "مبلغ فاكتور براي پيكربندي فعلي پذيرنده معتبر نمي باشد"},
            {-106, "سرويس وجود ندارد يا براي پذيرنده فعال نيست"},
            {-109, "هيچ آدرس كال بكي براي درخواست پيكربندي نشده است"},
            {-112, "شماره فاكتور نامعتبر يا تكراري است"},
            {-115, "پذيرنده فعال نيست يا پيكربندي پذيرنده غيرمعتبر است"}
        };

        private static IDictionary<int, string> VerificationDictionary => new Dictionary<int, string>
        {
            {500, "بازبيني تراكنش با موفقيت انجام شد"},
            {501, "پردازش هنوز انجام نشده است"},
            {502, "وضعيت تراكنش نامشخص است"},
            {503, "تراكنش اصلي ناموفق بوده است"},
            {504, "قبلا درخواست بازبيني براي اين تراكنش داده شده است"},
            {505, "قبلا درخواست تسويه براي اين تراكنش ارسال شده است"},
            {506, "قبلا درخواست بازگشت براي اين تراكنش ارسال شده است"},
            {507, "تراكنش در ليست تسويه قرار دارد"},
            {508, "تراكنش در ليست بازگشت قرار دارد"},
            {509, "امكان انجام عمليات به سبب وجود مشكل داخلي وجود ندارد"},
            {510, "هويت درخواست كننده عمليات نامعتبر است"}
        };

        private static IDictionary<int, string> ReconcilationDictionary => new Dictionary<int, string>
        {
            {600, "درخواست تسويه تراكنش با موفقيت ارسال شد"},
            {601, "پردازش هنوز انجام نشده است"},
            {602, "وضعيت تراكنش نامشخص است"},
            {603, "تراكنش اصلي ناموفق بوده است"},
            {604, "تراكنش بازبيني نشده است"},
            {605, "قبلا درخواست بازگشت براي اين تراكنش ارسال شده است"},
            {606, "قبلا درخواست تسويه براي اين تراكنش ارسال شده است"},
            {607, "امكان انجام عمليات به سبب وجود مشكل داخلي وجود ندارد"},
            {608, "تراكنش در ليست منتظر بازگشت ها وجود دارد"},
            {609, "تراكنش در ليست منتظر تسويه ها وجود دارد"},
            {610, "هويت درخواست كننده عمليات نامعتبر است"}
        };

        private static IDictionary<int, string> ReversalDictionary => new Dictionary<int, string>
        {
            {700, "درخواست بازگشت تراكنش با موفقيت ارسال شد"},
            {701, "پردازش هنوز انجام نشده است"},
            {702, "وضعيت تراكنش نامشخص است"},
            {703, "تراكنش اصلي ناموفق بوده است"},
            {704, "امكان بازگشت يك تراكنش بازبيني شده وجود ندارد"},
            {705, "قبلا درخواست بازگشت تراكنش براي اين تراكنش ارسال شده است"},
            {706, "قبلا درخواست تسويه براي اين تراكنش ارسال شده است"},
            {707, "امكان انجام عمليات به سبب وجود مشكل داخلي وجود ندارد"},
            {708, "تراكنش در ليست منتظر بازگشت ها وجود دارد"},
            {709, "تراكنش در ليست منتظر تسويه ها وجود دارد"}
        };
    }
}
