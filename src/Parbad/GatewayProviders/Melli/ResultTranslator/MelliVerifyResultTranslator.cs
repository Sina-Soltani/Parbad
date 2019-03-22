// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.GatewayProviders.Melli.ResultTranslator
{
    internal static class MelliVerifyResultTranslator
    {
        public static string Translate(int result, MessagesOptions options)
        {
            switch (result)
            {
                case 0:
                    return MelliVerifyResponseCodes.Code0;
                case -1:
                    return MelliVerifyResponseCodes.CodeMinus1;
                case 101:
                    return MelliVerifyResponseCodes.Code101;
                default:
                    return options.UnexpectedErrorText;
            }
        }
    }
}
