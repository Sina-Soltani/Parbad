// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Pasargad.Api.Models;

public class PasargadVerifyPaymentRequestModel
{
    public string InvoiceNumber { get; set; }

    public string InvoiceDate { get; set; }

    public string TerminalCode { get; set; }

    public string MerchantCode { get; set; }

    public decimal Amount { get; set; }

    public string Timestamp { get; set; }
}
