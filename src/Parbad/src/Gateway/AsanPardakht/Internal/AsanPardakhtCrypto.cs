// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Options;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Utilities;
using System.Net.Http;
using System.Threading.Tasks;

namespace Parbad.Gateway.AsanPardakht.Internal
{
    public class AsanPardakhtCrypto : IAsanPardakhtCrypto
    {
        private readonly HttpClient _httpClient;
        private readonly AsanPardakhtGatewayOptions _gatewayOptions;

        public AsanPardakhtCrypto(IHttpClientFactory httpClientFactory, IOptions<AsanPardakhtGatewayOptions> gatewayOptions)
        {
            _httpClient = httpClientFactory.CreateClient(AsanPardakhtGateway.Name);

            _gatewayOptions = gatewayOptions.Value;
        }

        public async Task<string> Encrypt(string input, string key, string iv)
        {
            var xmlBody = AsanPardakhtHelper.CreateEncryptData(key, iv, input);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.EncryptUrl, xmlBody)
                .ConfigureAwait(false);

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            var result = XmlHelper.GetNodeValueFromXml(response, "EncryptInAESResult", "http://tempuri.org/");

            return result;
        }

        public async Task<string> Decrypt(string input, string key, string iv)
        {
            var xmlBody = AsanPardakhtHelper.CreateDecryptData(key, iv, input);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.DecryptUrl, xmlBody)
                .ConfigureAwait(false);

            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

            var result = XmlHelper.GetNodeValueFromXml(response, "DecryptInAESResult", "http://tempuri.org/");

            return result;
        }
    }
}