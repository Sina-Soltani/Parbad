// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Parbad.Gateway.Pasargad.Api;
using Parbad.Gateway.Pasargad.Api.Models;
using Parbad.Net;

namespace Parbad.Gateway.Pasargad.Internal;

internal class PasargadApi : IPasargadApi
{
    private readonly HttpClient _httpClient;
    private readonly PasargadGatewayOptions _options;
    private readonly IPasargadCrypto _crypto;

    public PasargadApi(HttpClient httpClient,
                       IOptions<PasargadGatewayOptions> options,
                       IPasargadCrypto crypto)
    {
        _httpClient = httpClient;
        _crypto = crypto;
        _options = options.Value;
    }

    public Task<PasargadGetTokenResponseModel> GetToken(PasargadGetTokenRequestModel model,
                                                        string privateKey,
                                                        CancellationToken cancellationToken)
    {
        return PostJsonAsync<PasargadGetTokenResponseModel>(_options.ApiGetTokenUrl, model, privateKey, cancellationToken);
    }

    public Task<PasargadVerifyPaymentResponseModel> VerifyPayment(PasargadVerifyPaymentRequestModel model,
                                                                  string privateKey,
                                                                  CancellationToken cancellationToken)
    {
        return PostJsonAsync<PasargadVerifyPaymentResponseModel>(_options.ApiVerificationUrl, model, privateKey, cancellationToken);
    }

    public Task<PasargadRefundPaymentResponseModel> RefundPayment(PasargadRefundPaymentRequestModel model,
                                                                  string privateKey,
                                                                  CancellationToken cancellationToken)
    {
        return PostJsonAsync<PasargadRefundPaymentResponseModel>(_options.ApiRefundUrl, model, privateKey, cancellationToken);
    }

    private Task<TResponse> PostJsonAsync<TResponse>(string url,
                                                     object request,
                                                     string privateKey,
                                                     CancellationToken cancellationToken)
    {
        var json = JsonConvert.SerializeObject(request);

        var sign = _crypto.Encrypt(privateKey, json);

        AddDefaultHeaders(sign);

        return _httpClient.PostJsonAsync<TResponse>(url, request, cancellationToken: cancellationToken);
    }

    private void AddDefaultHeaders(string sign)
    {
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _httpClient.DefaultRequestHeaders.AddOrUpdate("Sign", sign);
    }
}
