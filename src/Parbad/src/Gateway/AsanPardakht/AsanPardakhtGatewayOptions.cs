// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.AsanPardakht
{
    public class AsanPardakhtGatewayOptions
    {
        public string PaymentPageUrl { get; set; } = "https://asan.shaparak.ir/";

        public string ApiBaseUrl { get; set; } = "https://ipgrest.asanpardakht.ir/v1/";

        public string ApiServerTimeUrl => $"{ApiBaseUrl}Time";

        public string ApiGetTokenUrl => $"{ApiBaseUrl}Token";

        public string ApiGetGetTransUrl => $"{ApiBaseUrl}GetTrans";

        public string ApiVerifyUrl => $"{ApiBaseUrl}Verify";

        public string ApiSettlementUrl => $"{ApiBaseUrl}Settlement";

        public string ApiCancelUrl => $"{ApiBaseUrl}Cancel";
    }
}
