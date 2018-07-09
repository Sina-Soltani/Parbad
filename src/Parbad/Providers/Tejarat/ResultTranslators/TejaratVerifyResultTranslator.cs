using Parbad.Infrastructure.Translating;

namespace Parbad.Providers.Tejarat.ResultTranslators
{
    internal class TejaratVerifyResultTranslator : IGatewayResultTranslator
    {
        public string Translate(object result)
        {
            int integerResult = (int)result;

            if (integerResult > 0)
            {
                return "پرداخت با موفقیت انجام گردید";
            }

            switch (integerResult)
            {
                case -20:
                    return "وجود کاراکترهای غیر مجاز در درخواست";

                case -30:
                    return "تراکنش قبلا برگشت خورده است";

                case -50:
                    return "طول رشته درخواست غیر مجاز است";

                case -51:
                    return "خطا در درخواست";

                case -80:
                    return "تراکنش مورد نظر یافت نشد";

                case -81:
                    return "خطای داخلی بانک";

                case -91:
                    return "تراکنش قبلا تأیید شده است";

                default:
                    return Resource.UnexpectedErrorText;
            }
        }
    }
}