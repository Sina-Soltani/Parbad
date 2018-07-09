using Parbad.Infrastructure.Translating;

namespace Parbad.Providers.Parbad.ResultTranslators
{
    internal class ParbadVirtualGatewayResultTranslator : IGatewayResultTranslator
    {
        public string Translate(object result)
        {
            switch (result.ToString())
            {
                case "false":
                    return "پرداخت انجام نشد";

                case "true":
                    return "پرداخت با موفقیت انجام گردید";

                default:
                    return Resource.UnexpectedErrorText;
            }
        }
    }
}