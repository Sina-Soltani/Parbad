// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.ZarinPal;

public class ZarinPalGatewayOptions
{
    public string ApiRequestUrl { get; set; } = "https://payment.zarinpal.com/pg/v4/payment/request.json";

    public string ApiVerificationUrl { get; set; } = "https://payment.zarinpal.com/pg/v4/payment/verify.json";

    /// <summary>
    /// ZarinPal's GraphQL API, which refunds use. ZarinPal has no sandbox for it.
    /// </summary>
    public string ApiGraphQlUrl { get; set; } = "https://next.zarinpal.com/api/v4/graphql/";

    /// <summary>
    /// How a refund is paid back to the customer. The default is <see cref="ZarinPalRefundMethod.Paya"/>.
    /// </summary>
    public ZarinPalRefundMethod RefundMethod { get; set; } = ZarinPalRefundMethod.Paya;

    /// <summary>
    /// The reason ZarinPal records for a refund. The default is <see cref="ZarinPalRefundReason.CustomerRequest"/>.
    /// </summary>
    public ZarinPalRefundReason RefundReason { get; set; } = ZarinPalRefundReason.CustomerRequest;

    public string PaymentPageUrl { get; set; } = "https://payment.zarinpal.com/pg/StartPay/";

    public string SandboxApiRequestUrl { get; set; } = "https://sandbox.zarinpal.com/pg/v4/payment/request.json";

    public string SandboxApiVerificationUrl { get; set; } = "https://sandbox.zarinpal.com/pg/v4/payment/verify.json";

    public string SandboxPaymentPageUrl { get; set; } = "https://sandbox.zarinpal.com/pg/StartPay/";
}
