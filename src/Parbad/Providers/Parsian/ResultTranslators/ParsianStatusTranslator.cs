using Parbad.Infrastructure.Translating;

namespace Parbad.Providers.Parsian.ResultTranslators
{
    internal class ParsianStatusTranslator : IGatewayResultTranslator
    {
        public string Translate(object result)
        {
            switch ((byte)result)
            {
                case 0:
                    return "تراكنش با موفقيت انجام شد";
                case 1:
                    return "وضعيت بلا تكليف";
                case 20:
                    return "پين فروشنده درست نمي باشد";
                case 22:
                    return "پين فروشنده درست نمي باشد";
                case 30:
                    return "عمليات قبلا با موفقيت انجام شده است";
                case 34:
                    return "شماره تراکنش فروشنده درست نميباشد";
                default:
                    return Resource.UnexpectedErrorText;
            }
        }
    }
}