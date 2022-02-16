// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Zibal
{
    public class ZibalGatewayOptions
    {
        public string RequestURl { get; set; } = "https://gateway.zibal.ir/v1/request";

        public string VerifyURl { get; set; } = "https://gateway.zibal.ir/v1/verify";

        public string PaymentUrl { get; set; } = "https://gateway.zibal.ir/start";
    }
}
