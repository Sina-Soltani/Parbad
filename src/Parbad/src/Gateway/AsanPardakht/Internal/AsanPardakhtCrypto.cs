// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Options;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Utilities;
using System.Net.Http;
using System.Threading.Tasks;

#pragma warning disable CS0618 // The SOAP crypto service and its options are obsolete; kept until 4.0 for applications that use it.

namespace Parbad.Gateway.AsanPardakht.Internal
{
    /// <summary>
    /// The remote encryption service of AsanPardakht's former SOAP API. The gateway no longer uses it.
    /// </summary>
    internal class AsanPardakhtCrypto : IAsanPardakhtCrypto
    {
        private readonly HttpClient _httpClient;
        private readonly AsanPardakhtGatewayOptions _gatewayOptions;

        public AsanPardakhtCrypto(IHttpClientFactory httpClientFactory, IOptions<AsanPardakhtGatewayOptions> gatewayOptions)
        {
            // The gateway's own client, so WithHttpClient, WithProxy and the timeout apply to these calls too.
            _httpClient = httpClientFactory.CreateClient(nameof(AsanPardakhtGateway));

            _gatewayOptions = gatewayOptions.Value;
        }

        public async Task<string> Encrypt(string input, string key, string iv)
        {
            var xmlBody = CreateEnvelope("EncryptInAES", "toBeEncrypted", key, iv, input);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.EncryptUrl, xmlBody)
                .ConfigureAwait(false);

            var response = await responseMessage.ReadSoapAsync().ConfigureAwaitFalse();

            return XmlHelper.GetNodeValueFromXml(response, "EncryptInAESResult", "http://tempuri.org/");
        }

        public async Task<string> Decrypt(string input, string key, string iv)
        {
            var xmlBody = CreateEnvelope("DecryptInAES", "toBeDecrypted", key, iv, input);

            var responseMessage = await _httpClient
                .PostXmlAsync(_gatewayOptions.DecryptUrl, xmlBody)
                .ConfigureAwait(false);

            var response = await responseMessage.ReadSoapAsync().ConfigureAwaitFalse();

            return XmlHelper.GetNodeValueFromXml(response, "DecryptInAESResult", "http://tempuri.org/");
        }

        private static string CreateEnvelope(string operation, string inputElement, string key, string iv, string input) =>
            "<soap:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>" +
            "<soap:Body>" +
            $"<{operation} xmlns='http://tempuri.org/'>" +
            $"<aesKey>{XmlHelper.EncodeXmlValue(key)}</aesKey>" +
            $"<aesVector>{XmlHelper.EncodeXmlValue(iv)}</aesVector>" +
            $"<{inputElement}>{XmlHelper.EncodeXmlValue(input)}</{inputElement}>" +
            $"</{operation}>" +
            "</soap:Body>" +
            "</soap:Envelope>";
    }
}
