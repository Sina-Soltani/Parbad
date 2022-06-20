// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.AsanPardakht
{
    public class AsanPardakhtGatewayOptions
    {
        public string PaymentPageUrl { get; set; } = "https://asan.shaparak.ir/";

        public string BaseApiUrl = "https://ipgrest.asanpardakht.ir/v1/";

        public string GetTokenUrl => $"{BaseApiUrl}Token";

        public string TimeUrl => $"{BaseApiUrl}Time";

        public string GetTransUrl => $"{BaseApiUrl}TranResult";

        public string VerifyUrl => $"{BaseApiUrl}Verify";

        public string SettlementUrl => $"{BaseApiUrl}Settlement";

        public string CancelUrl => $"{BaseApiUrl}Cancel";
    }
}
