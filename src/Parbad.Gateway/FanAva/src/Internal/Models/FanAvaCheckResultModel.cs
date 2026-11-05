// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.FanAva.Internal.Models;

internal class FanAvaCheckResultModel
{
    /// <summary>
    /// The outcome of <see cref="FanAvaHelper.CreateCheckResult"/>, not a field of the bank's response.
    /// </summary>
    [JsonIgnore]
    public bool IsSucceed { get; set; }

    [JsonIgnore]
    public IPaymentVerifyResult VerifyResult { get; set; }

    [JsonProperty("Result")]
    public string Result { get; set; }

    [JsonProperty("ReferenceNum")]
    public string InvoiceNumber { get; set; }

    [JsonProperty("Amount")]
    public decimal Amount { get; set; }
}