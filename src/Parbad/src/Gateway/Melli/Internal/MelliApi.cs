// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Parbad.Gateway.Melli.Api;
using Parbad.Gateway.Melli.Api.Models;
using Parbad.Net;

namespace Parbad.Gateway.Melli.Internal;

internal class MelliApi : IMelliApi
{
    private readonly HttpClient _httpClient;
    private readonly MelliGatewayOptions _gatewayOptions;

    public MelliApi(HttpClient httpClient, IOptions<MelliGatewayOptions> gatewayOptions)
    {
        _httpClient = httpClient;
        _gatewayOptions = gatewayOptions.Value;
    }

    public Task<MelliApiRequestResultModel> Request(MelliApiRequestModel model, CancellationToken cancellationToken = default)
    {
        return PostJsonAsync<MelliApiRequestResultModel>(_gatewayOptions.ApiRequestUrl, model, cancellationToken);
    }

    public Task<MelliApiVerifyResultModel> Verify(MelliApiVerifyModel model, CancellationToken cancellationToken = default)
    {
        return PostJsonAsync<MelliApiVerifyResultModel>(_gatewayOptions.ApiVerificationUrl, model, cancellationToken);
    }

    private Task<T> PostJsonAsync<T>(string url, object data, CancellationToken cancellationToken)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
                                     {
                                         ContractResolver = new CamelCasePropertyNamesContractResolver()
                                     };

        return _httpClient.PostJsonAsync<T>(url, data, jsonSerializerSettings, cancellationToken);
    }
}
