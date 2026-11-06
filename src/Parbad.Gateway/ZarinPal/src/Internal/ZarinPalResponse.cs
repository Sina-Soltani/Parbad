// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parbad.Exceptions;
using Parbad.Internal;
using Parbad.Net;
using Parbad.Options;

namespace Parbad.Gateway.ZarinPal.Internal;

/// <summary>
/// A response of the ZarinPal API v4.
/// <para>
/// ZarinPal changes the JSON types with the outcome: a success has "data" as an object and "errors" as an empty
/// array; an error has "data" as an empty array or object and "errors" as an object (sometimes an array), whatever
/// the HTTP status code. The body is therefore read by its shape and never deserialized straight into a model.
/// </para>
/// </summary>
internal class ZarinPalResponse<TData> where TData : class
{
    private ZarinPalResponse(TData? data, int? errorCode, string? errorMessage)
    {
        Data = data;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    /// <summary>The "data" object, or <see langword="null"/> when ZarinPal answered with an error.</summary>
    public TData? Data { get; }

    public bool IsError => Data == null;

    public int? ErrorCode { get; }

    public string? ErrorMessage { get; }

    /// <exception cref="GatewayResponseException">
    /// It is not known whether ZarinPal processed the call: a server error, a body that is not JSON (e.g. an HTML
    /// error page from a proxy), or a successful status code with neither data nor errors.
    /// </exception>
    public static async Task<ZarinPalResponse<TData>> ReadAsync(HttpResponseMessage responseMessage, MessagesOptions messagesOptions)
    {
        // A server error says nothing about whether ZarinPal processed the call, so it is not reported as a
        // rejection: the payment stays open for a retry.
        if ((int)responseMessage.StatusCode >= 500) throw CreateException(responseMessage, "server error");

        var body = await responseMessage.ReadJsonStringAsync().ConfigureAwaitFalse();

        var json = Parse(body) as JObject ?? throw CreateException(responseMessage, "not a JSON object");

        if (json["data"] is JObject data && data["code"] != null)
        {
            return new(data.ToObject<TData>(), null, null);
        }

        var error = json["errors"] switch
        {
            JObject errorObject => errorObject,
            JArray errorArray => errorArray.OfType<JObject>().FirstOrDefault(),
            _ => null
        };

        if (error == null && responseMessage.IsSuccessStatusCode)
        {
            throw CreateException(responseMessage, "neither data nor errors");
        }

        var errorCode = int.TryParse(error?["code"]?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var code)
            ? code
            : (int?)null;

        string message;

        if (errorCode != null)
        {
            message = ZarinPalCodeTranslator.Translate(errorCode.Value, messagesOptions);
        }
        else if (error?["message"] is JValue { Type: JTokenType.String } errorMessage && !string.IsNullOrWhiteSpace((string?)errorMessage))
        {
            message = (string)errorMessage!;
        }
        else
        {
            message = $"ZarinPal HTTP request failed. Status code {responseMessage.StatusCode}";
        }

        return new(null, errorCode, message);
    }

    // Dates are kept as strings, as the models declare them.
    private static JToken Parse(string body)
    {
        using var reader = new JsonTextReader(new StringReader(body)) { DateParseHandling = DateParseHandling.None };

        return JToken.ReadFrom(reader);
    }

    private static GatewayResponseException CreateException(HttpResponseMessage responseMessage, string reason) =>
        new(responseMessage.StatusCode, responseMessage.Content?.Headers.ContentType?.MediaType, reason);
}
