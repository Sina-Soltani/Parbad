// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.AsanPardakht.Internal.Models;

/// <summary>
/// The answer of TranResult: the bank's record of the payment of an invoice.
/// </summary>
internal class AsanPardakhtTransactionResult
{
    [JsonProperty("cardNumber")]
    public string CardNumber { get; set; }

    [JsonProperty("rrn")]
    public string Rrn { get; set; }

    /// <summary>The RefId (token) that the payment was started with.</summary>
    [JsonProperty("refID")]
    public string RefId { get; set; }

    /// <summary>The amount in rials.</summary>
    [JsonProperty("amount")]
    public string Amount { get; set; }

    [JsonProperty("payGateTranID")]
    public string PayGateTranId { get; set; }

    [JsonProperty("salesOrderID")]
    public string SalesOrderId { get; set; }

    [JsonProperty("serviceStatusCode")]
    public string ServiceStatusCode { get; set; }
}