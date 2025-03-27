// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parbad.Gateway.Saman.Api;
using Parbad.Gateway.Saman.Api.Models;
using Parbad.Net;

namespace Parbad.Gateway.Saman.Internal;

internal class SamanApi : ISamanApi
{
    private readonly HttpClient _httpClient;
    private readonly SamanGatewayOptions _gatewayOptions;

    public SamanApi(HttpClient httpClient, IOptions<SamanGatewayOptions> gatewayOptions)
    {
        _httpClient = httpClient;
        _gatewayOptions = gatewayOptions.Value;
    }

    public Task<SamanTokenResponse> RequestToken(SamanTokenRequest request,
                                                      CancellationToken cancellationToken = default)
    {
        return PostJsonAsync<SamanTokenResponse>(_gatewayOptions.ApiTokenUrl, request, cancellationToken);
    }

    public Task<SamanVerificationAndReverseResponse> Verify(SamanVerificationRequest request,
                                                           CancellationToken cancellationToken = default)
    {
        return PostJsonAsync<SamanVerificationAndReverseResponse>(_gatewayOptions.ApiVerificationUrl, request, cancellationToken);
    }

    public Task<SamanVerificationAndReverseResponse> Reverse(SamanReverseRequest request,
                                                           CancellationToken cancellationToken = default)
    {
        return PostJsonAsync<SamanVerificationAndReverseResponse>(_gatewayOptions.ApiReverseUrl, request, cancellationToken);
    }

    private Task<T> PostJsonAsync<T>(string url,
                                     object data,
                                     CancellationToken cancellationToken)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
                                     {
                                         ContractResolver = new CamelCasePropertyNamesContractResolver()
                                     };

        return _httpClient.PostJsonAsync<T>(url, data, jsonSerializerSettings, cancellationToken);
    }
}
