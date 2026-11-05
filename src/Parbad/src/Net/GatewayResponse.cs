// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parbad.Exceptions;
using Parbad.Internal;

namespace Parbad.Net;

/// <summary>
/// Reads the JSON responses of gateways.
/// <para>
/// A response the gateway can read, including a JSON error body with a non-success status code, is a definite
/// answer from the bank and is returned. A response that cannot be read throws
/// <see cref="GatewayResponseException"/> instead of a <see cref="NullReferenceException"/> or a parsing
/// exception, so the payment stays open for a retry.
/// </para>
/// </summary>
public static class GatewayResponse
{
    /// <summary>
    /// Reads the body of <paramref name="responseMessage"/> as JSON.
    /// </summary>
    /// <exception cref="GatewayResponseException">The body is empty or not valid JSON.</exception>
    public static async Task<TResult> ReadJsonAsync<TResult>(this HttpResponseMessage responseMessage)
    {
        if (responseMessage == null) throw new ArgumentNullException(nameof(responseMessage));

        var body = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

        return DeserializeJson<TResult>(responseMessage, body);
    }

    /// <summary>
    /// Deserializes <paramref name="body"/>, the already read body of <paramref name="responseMessage"/>.
    /// </summary>
    /// <exception cref="GatewayResponseException">The body is empty or not valid JSON.</exception>
    public static TResult DeserializeJson<TResult>(HttpResponseMessage responseMessage, string body)
    {
        if (responseMessage == null) throw new ArgumentNullException(nameof(responseMessage));

        TResult result;

        try
        {
            result = JsonConvert.DeserializeObject<TResult>(body ?? string.Empty);
        }
        catch (JsonException)
        {
            throw CreateException(responseMessage, "not valid JSON");
        }

        if (result == null) throw CreateException(responseMessage, "empty body");

        return result;
    }

    /// <summary>
    /// Reads the body of <paramref name="responseMessage"/> and checks that it is JSON, for gateways that inspect
    /// the JSON themselves (e.g. an error body with a non-success status code).
    /// </summary>
    /// <exception cref="GatewayResponseException">The body is empty or not valid JSON.</exception>
    public static async Task<string> ReadJsonStringAsync(this HttpResponseMessage responseMessage)
    {
        if (responseMessage == null) throw new ArgumentNullException(nameof(responseMessage));

        var body = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

        try
        {
            JToken.Parse(body ?? string.Empty);
        }
        catch (JsonException)
        {
            throw CreateException(responseMessage, "not valid JSON");
        }

        return body;
    }

    /// <summary>
    /// Reads the body of <paramref name="responseMessage"/> and checks that it is a SOAP envelope. A SOAP fault is
    /// an envelope too, so it is returned and the gateway reports it as a failure.
    /// </summary>
    /// <exception cref="GatewayResponseException">The body is not a SOAP envelope, e.g. an HTML error page.</exception>
    public static async Task<string> ReadSoapAsync(this HttpResponseMessage responseMessage)
    {
        if (responseMessage == null) throw new ArgumentNullException(nameof(responseMessage));

        var body = await responseMessage.Content.ReadAsStringAsync().ConfigureAwaitFalse();

        if (!IsSoapEnvelope(body)) throw CreateException(responseMessage, "not a SOAP envelope");

        return body;
    }

    private static bool IsSoapEnvelope(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return false;

        try
        {
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };

            using var reader = XmlReader.Create(new StringReader(body), settings);

            reader.MoveToContent();

            return reader.NodeType == XmlNodeType.Element && reader.LocalName == "Envelope";
        }
        catch (XmlException)
        {
            return false;
        }
    }

    private static GatewayResponseException CreateException(HttpResponseMessage responseMessage, string reason) =>
        new GatewayResponseException(responseMessage.StatusCode, responseMessage.Content?.Headers.ContentType?.MediaType, reason);
}