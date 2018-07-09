using Parbad.Infrastructure.Translating;

namespace Parbad.Providers.Saman.ResultTranslators
{
    internal class SamanResultTranslator : IGatewayResultTranslator
    {
        public string Translate(object result)
        {
            if (result == null)
            {
                return Resource.UnexpectedErrorText;
            }

            var integerResult = (int)result;

            if (integerResult >= 0)
            {
                return "تراکنش با موفقیت انجام گردید";
            }

            switch (integerResult)
            {
                case -1:
                    return "خطای داخلی شبکه مالی";

                case -2:
                    return "سپرده ها برابر نیستند";

                case -3:
                    return "ورودی ها حاوی کاراکترهای غیرمجاز هستند";

                case -4:
                    return "کد پذیرنده و یا کلمه عبور آن اشتباه است";

                case -5:
                    return "Database Exception";

                case -6:
                    return "سند قبلا برگشت کامل یافته است";

                case -7:
                    return "رسید دیجیتالی تهی است";

                case -8:
                    return "طول ورودی ها بیشتر از حد مجاز است";

                case -9:
                    return "وجود کاراکترهای غیرمجاز در مبلغ برگشتی";

                case -10:
                    return "رسید دیجیتالی حاوی کاراکترهای غیرمجاز است";

                case -11:
                    return "طول ورودی ها کمتر از حد مجاز است";

                case -12:
                    return "مبلغ برگشتی منفی است";

                case -13:
                    return "مبلغ برگشتی برای برگشت جزئی بیش از مبلغ برگشت نخورده رسیده دیجیتالی است";

                case -14:
                    return "چنین تراکنشی تعریف نشده است";

                case -15:
                    return "مبلغ برگشتی بصورت اعشاری داده شده است";

                case -16:
                    return "خطای داخلی سیستم";

                case -17:
                    return "برگشت زدن جزئی تراکنشی که با کارتی غیر از بانک سامان انجام شده است";

                case -18:
                    return "IP Address پذیرنده نامعتبر است";
                default:
                    return Resource.UnexpectedErrorText;
            }
        }
    }
}