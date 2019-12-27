// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.Melli.Internal.ResultTranslator
{
    internal static class MelliRequestResultTranslator
    {
        public static string Translate(int? result, MessagesOptions options)
        {
            switch (result)
            {
                case 0:
                    return MelliRequestResponseCodes.Code0;
                case 3:
                    return MelliRequestResponseCodes.Code3;
                case 23:
                    return MelliRequestResponseCodes.Code23;
                case 58:
                    return MelliRequestResponseCodes.Code58;
                case 61:
                    return MelliRequestResponseCodes.Code61;
                case 1000:
                    return MelliRequestResponseCodes.Code1001;
                case 1001:
                    return MelliRequestResponseCodes.Code1001;
                case 1002:
                    return MelliRequestResponseCodes.Code1002;
                case 1003:
                    return MelliRequestResponseCodes.Code1003;
                case 1004:
                    return MelliRequestResponseCodes.Code1004;
                case 1005:
                    return MelliRequestResponseCodes.Code1005;
                case 1006:
                    return MelliRequestResponseCodes.Code1006;
                case 1011:
                    return MelliRequestResponseCodes.Code1011;
                case 1012:
                    return MelliRequestResponseCodes.Code1012;
                case 1015:
                    return MelliRequestResponseCodes.Code1015;
                case 1017:
                    return MelliRequestResponseCodes.Code1017;
                case 1018:
                    return MelliRequestResponseCodes.Code1018;
                case 1019:
                    return MelliRequestResponseCodes.Code1019;
                case 1020:
                    return MelliRequestResponseCodes.Code1020;
                case 1023:
                    return MelliRequestResponseCodes.Code1023;
                case 1024:
                    return MelliRequestResponseCodes.Code1024;
                case 1025:
                    return MelliRequestResponseCodes.Code1025;
                case 1026:
                    return MelliRequestResponseCodes.Code1026;
                case 1027:
                    return MelliRequestResponseCodes.Code1027;
                case 1028:
                    return MelliRequestResponseCodes.Code1028;
                case 1029:
                    return MelliRequestResponseCodes.Code1029;
                case 1030:
                    return MelliRequestResponseCodes.Code1030;
                case 1031:
                    return MelliRequestResponseCodes.Code1031;
                case 1032:
                    return MelliRequestResponseCodes.Code1032;
                case 1033:
                    return MelliRequestResponseCodes.Code1033;
                case 1036:
                    return MelliRequestResponseCodes.Code1036;
                case 1037:
                    return MelliRequestResponseCodes.Code1037;
                case 1053:
                    return MelliRequestResponseCodes.Code1053;
                case 1055:
                    return MelliRequestResponseCodes.Code1055;
                case 1056:
                    return MelliRequestResponseCodes.Code1056;
                case 1058:
                    return MelliRequestResponseCodes.Code1058;
                case 1061:
                    return MelliRequestResponseCodes.Code1061;
                case 1064:
                    return MelliRequestResponseCodes.Code1064;
                case 1065:
                    return MelliRequestResponseCodes.Code1065;
                case 1066:
                    return MelliRequestResponseCodes.Code1066;
                case 1068:
                    return MelliRequestResponseCodes.Code1068;
                case 1072:
                    return MelliRequestResponseCodes.Code1072;
                case 1101:
                    return MelliRequestResponseCodes.Code1101;
                case 1103:
                    return MelliRequestResponseCodes.Code1103;
                case 1104:
                    return MelliRequestResponseCodes.Code1104;
                default:
                    return options.UnexpectedErrorText;
            }
        }
    }
}
