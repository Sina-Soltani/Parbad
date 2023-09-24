// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Saman;

public class SamanGatewayOptions
{
    public string PaymentPageUrl { get; set; } = "https://sep.shaparak.ir/OnlinePG/OnlinePG";

    public string ApiTokenUrl { get; set; } = "https://sep.shaparak.ir/onlinepg/onlinepg";

    public string ApiVerificationUrl { get; set; } = "https://sep.shaparak.ir/verifyTxnRandomSessionkey/ipg/VerifyTransaction";

    public string ApiReverseUrl { get; set; } = "https://sep.shaparak.ir/verifyTxnRandomSessionkey/ipg/ReverseTransaction";
}
