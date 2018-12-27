using Parbad.Infrastructure.Translating;

namespace Parbad.Providers.Melli.ResultTranslator
{
    internal class MelliVerifyResultTranslator : IGatewayResultTranslator
    {
        public string Translate(object result)
        {
            switch ((int)result)
            {
                case 0:
                    return MelliVerifyResponseCodes.Code0;
                case -1:
                    return MelliVerifyResponseCodes.CodeMinus1;
                case 101:
                    return MelliVerifyResponseCodes.Code101;
                default:
                    return Resource.UnexpectedErrorText;
            }
        }
    }
}
