// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Parbad.Gateway.Pasargad.Api;
using Parbad.Gateway.Pasargad.Api.Models;
using Parbad.Net;

namespace Parbad.Gateway.Pasargad.Internal;

public class PasargadApi : IPasargadApi
{
    private readonly HttpClient _httpClient;
    private readonly PasargadGatewayOptions _options;

    public PasargadApi(HttpClient httpClient,
                       IOptions<PasargadGatewayOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public virtual Task<PasargadGetTokenResponseModel> GetToken(PasargadGetTokenRequestModel model,
                                                        CancellationToken cancellationToken)
    {
        return PostJsonAsync<PasargadGetTokenResponseModel>(_options.ApiGetTokenUrl, model, "", cancellationToken);
    }

    public virtual Task<PasargadPurchaseResponseModel> PurchasePayment(PasargadPurchaseRequestModel model,
                                                               string token,
                                                               CancellationToken cancellationToken)
    {
        return PostJsonAsync<PasargadPurchaseResponseModel>(_options.ApiPurchaseUrl, model, token, cancellationToken);
    }

    public virtual Task<PasargadVerifyPaymentResponseModel> VerifyPayment(PasargadVerifyPaymentRequestModel model,
                                                                          string token,
                                                                          CancellationToken cancellationToken)
    {
        return PostJsonAsync<PasargadVerifyPaymentResponseModel>(_options.ApiVerificationUrl, model, token, cancellationToken);
    }

    public virtual Task<PasargadRefundPaymentResponseModel> RefundPayment(PasargadRefundPaymentRequestModel model,
                                                                          string token,
                                                                          CancellationToken cancellationToken)
    {
        return PostJsonAsync<PasargadRefundPaymentResponseModel>(_options.ApiReverseUrl, model, token, cancellationToken);
    }

    private Task<TResponse> PostJsonAsync<TResponse>(string url,
                                                     object request,
                                                     string token,
                                                     CancellationToken cancellationToken)
    {
        AddDefaultHeaders(token);

        return _httpClient.PostJsonAsync<TResponse>(url, request, cancellationToken: cancellationToken);
    }

    private void AddDefaultHeaders(string token)
    {
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
