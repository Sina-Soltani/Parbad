// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad.Internal;

/// <inheritdoc />
/// <remarks>
/// Results cannot be changed after they are created. Gateways create them with the factory methods or an object
/// initializer; a copy with other values is made with <c>result with { ... }</c>.
/// </remarks>
public record PaymentResult : IPaymentResult
{
    /// <summary>
    /// Initializes an instance of <see cref="PaymentResult"/>.
    /// </summary>
    public PaymentResult()
    {
        AdditionalData = new Dictionary<string, object>();
        DatabaseAdditionalData = new Dictionary<string, string>();
    }

    /// <inheritdoc />
    public long TrackingNumber { get; init; }

    /// <inheritdoc />
    public Money Amount { get; init; }

    /// <inheritdoc />
    public virtual bool IsSucceed { get; init; }

    /// <inheritdoc />
    public string GatewayName { get; init; } = null!;

    /// <inheritdoc />
    public string? GatewayAccountName { get; init; }

    /// <inheritdoc />
    public string? GatewayResponseCode { get; init; }

    /// <inheritdoc />
    public string? Message { get; init; }

    /// <inheritdoc />
    public IDictionary<string, object> AdditionalData { get; init; }

    public IDictionary<string, string> DatabaseAdditionalData { get; init; }
}