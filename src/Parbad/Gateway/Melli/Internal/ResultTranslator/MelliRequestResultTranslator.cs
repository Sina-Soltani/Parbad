// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.Melli.Internal.ResultTranslator
{
    internal static class MelliRequestResultTranslator
    {
        public static string Translate(int? result, MessagesOptions options)
        {
            return result switch
            {
                0 => MelliRequestResponseCodes.Code0,
                3 => MelliRequestResponseCodes.Code3,
                23 => MelliRequestResponseCodes.Code23,
                58 => MelliRequestResponseCodes.Code58,
                61 => MelliRequestResponseCodes.Code61,
                1000 => MelliRequestResponseCodes.Code1001,
                1001 => MelliRequestResponseCodes.Code1001,
                1002 => MelliRequestResponseCodes.Code1002,
                1003 => MelliRequestResponseCodes.Code1003,
                1004 => MelliRequestResponseCodes.Code1004,
                1005 => MelliRequestResponseCodes.Code1005,
                1006 => MelliRequestResponseCodes.Code1006,
                1011 => MelliRequestResponseCodes.Code1011,
                1012 => MelliRequestResponseCodes.Code1012,
                1015 => MelliRequestResponseCodes.Code1015,
                1017 => MelliRequestResponseCodes.Code1017,
                1018 => MelliRequestResponseCodes.Code1018,
                1019 => MelliRequestResponseCodes.Code1019,
                1020 => MelliRequestResponseCodes.Code1020,
                1023 => MelliRequestResponseCodes.Code1023,
                1024 => MelliRequestResponseCodes.Code1024,
                1025 => MelliRequestResponseCodes.Code1025,
                1026 => MelliRequestResponseCodes.Code1026,
                1027 => MelliRequestResponseCodes.Code1027,
                1028 => MelliRequestResponseCodes.Code1028,
                1029 => MelliRequestResponseCodes.Code1029,
                1030 => MelliRequestResponseCodes.Code1030,
                1031 => MelliRequestResponseCodes.Code1031,
                1032 => MelliRequestResponseCodes.Code1032,
                1033 => MelliRequestResponseCodes.Code1033,
                1036 => MelliRequestResponseCodes.Code1036,
                1037 => MelliRequestResponseCodes.Code1037,
                1053 => MelliRequestResponseCodes.Code1053,
                1055 => MelliRequestResponseCodes.Code1055,
                1056 => MelliRequestResponseCodes.Code1056,
                1058 => MelliRequestResponseCodes.Code1058,
                1061 => MelliRequestResponseCodes.Code1061,
                1064 => MelliRequestResponseCodes.Code1064,
                1065 => MelliRequestResponseCodes.Code1065,
                1066 => MelliRequestResponseCodes.Code1066,
                1068 => MelliRequestResponseCodes.Code1068,
                1072 => MelliRequestResponseCodes.Code1072,
                1101 => MelliRequestResponseCodes.Code1101,
                1103 => MelliRequestResponseCodes.Code1103,
                1104 => MelliRequestResponseCodes.Code1104,
                _ => options.UnexpectedErrorText
            };
        }
    }
}
