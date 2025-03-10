// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Parbad.Gateway.Pasargad.Api.Models;

namespace Parbad.Gateway.Pasargad.Api;

/// <summary>
/// API provided by Pasargad Bank.
/// </summary>
public interface IPasargadApi
{
    /// <summary>
    /// Gets a token to start a payment request.
    /// </summary>
    Task<PasargadGetTokenResponseModel> GetToken(PasargadGetTokenRequestModel model,
                                                 CancellationToken cancellationToken);

    /// <summary>
    /// Send payment request to Pasargad Bank.
    /// </summary>
    Task<PasargadPurchaseResponseModel> PurchasePayment(PasargadPurchaseRequestModel model, string token,
        CancellationToken cancellationToken);

    /// <summary>
    /// Verifies a payment.
    /// </summary>
    Task<PasargadVerifyPaymentResponseModel> VerifyPayment(PasargadVerifyPaymentRequestModel model, string token,
                                                           CancellationToken cancellationToken);

    /// <summary>
    /// Refunds an already paid invoice.
    /// </summary>
    Task<PasargadRefundPaymentResponseModel> RefundPayment(PasargadRefundPaymentRequestModel model, string token,
                                                           CancellationToken cancellationToken);
}
