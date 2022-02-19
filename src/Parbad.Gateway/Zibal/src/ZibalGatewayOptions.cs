// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Zibal
{
    public class ZibalGatewayOptions
    {
        public string ApiRequestUrl { get; set; } = "https://gateway.zibal.ir/v1/request";

        public string ApiVerificationUrl { get; set; } = "https://gateway.zibal.ir/v1/verify";

        public string PaymentPageUrl { get; set; } = "https://gateway.zibal.ir/start";
    }
}
