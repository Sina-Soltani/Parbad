// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.Pasargad.Api.Models;

public class PasargadPurchaseRequestModel
{
    public string Invoice { get; set; }

    public string InvoiceDate { get; set; }

    public decimal Amount { get; set; }

    public int ServiceCode { get; set; } = 8;

    public string ServiceType { get; set; } = "PURCHASE";

    public string Description { get; set; }

    public string PaymentCode { get; set; }

    public string MobileNumber { get; set; }

    public string NationalCode { get; set; }

    public string PayerName { get; set; }





    public string TerminalCode { get; set; }

    public string MerchantCode { get; set; }

    public string RedirectAddress { get; set; }

    public string Timestamp { get; set; }

    public string Action => "1003";

    public string Mobile { get; set; }

    public string Email { get; set; }

    public string MerchantName { get; set; }

    [JsonProperty("PIDN")]
    public string Pidn { get; set; }
}

