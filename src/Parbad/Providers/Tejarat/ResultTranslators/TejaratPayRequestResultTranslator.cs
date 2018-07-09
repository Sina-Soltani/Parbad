using Parbad.Infrastructure.Translating;

namespace Parbad.Providers.Tejarat.ResultTranslators
{
    internal class TejaratPayRequestResultTranslator : IGatewayResultTranslator
    {
        public string Translate(object result)
        {
            switch ((int)result)
            {
                case 100:
                    return "موفقیت تراکنش";

                case 110:
                    return "انصراف دارنده کارت";

                case 120:
                    return "موجودی حساب کافی نیست";

                case 130:
                    return "اطلاعات کارت اشتباه است";

                case 131:
                    return "رمز اشتباه است";

                case 132:
                    return "کارت مسدود شده است";

                case 133:
                    return "کارت منقضی شده است";

                case 140:
                    return "زمان مورد نظر به پایان رسیده است";

                case 150:
                    return "خطای داخلی بانک";

                case 160:
                    return "خطا در اطلاعات CVV2 یا ExpDate";

                case 166:
                    return "بانک صادر کننده کارت شما مجوز تراکنش را صادر نکرده است";

                case 200:
                    return "مبلغ تراکنش بیشتر از سقف مجاز برای هر تراکنش می باشد";

                case 201:
                    return "مبلغ تراکنش بیشتر از سقف مجاز در روز می باشد";

                case 202:
                    return "مبلغ تراکنش بیشتر از سقف مجاز در ماه می باشد";

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