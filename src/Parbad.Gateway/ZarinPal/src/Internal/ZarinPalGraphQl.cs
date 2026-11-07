// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parbad.Exceptions;
using Parbad.Internal;
using Parbad.Net;

namespace Parbad.Gateway.ZarinPal.Internal;

/// <summary>
/// ZarinPal's GraphQL API (next.zarinpal.com/api/v4/graphql/), which refunds use. It takes the access token of the
/// ZarinPal panel as a Bearer token, and has no sandbox.
/// </summary>
internal static class ZarinPalGraphQl
{
    /// <summary>The merchant's terminals; <c>key</c> is the merchant id.</summary>
    public const string TerminalsQuery = "query { Terminals { id key } }";

    /// <summary>A payment ("session") by its ref_id.</summary>
    public const string SessionsQuery =
        "query Sessions($terminal_id: ID!, $reference_id: String) { " +
        "Session(terminal_id: $terminal_id, reference_id: $reference_id) { id authority reference_id status amount } }";

    public const string AddRefundMutation =
        "mutation AddRefund($session_id: ID!, $amount: BigInteger!, $description: String, $method: InstantPayoutActionTypeEnum, $reason: RefundReasonEnum) { " +
        "resource: AddRefund(session_id: $session_id, amount: $amount, description: $description, method: $method, reason: $reason) { " +
        "terminal_id, id, amount, timeline { refund_amount, refund_time, refund_status } } }";

    /// <summary>
    /// Sends a query or mutation. A GraphQL error is returned as <see cref="GraphQlResponse.Error"/>.
    /// </summary>
    /// <exception cref="GatewayResponseException">A server error, or a body that is not a JSON object.</exception>
    public static async Task<GraphQlResponse> SendAsync(HttpClient httpClient,
                                                        string url,
                                                        string accessToken,
                                                        string query,
                                                        object? variables,
                                                        CancellationToken cancellationToken)
    {
        var body = JsonConvert.SerializeObject(new { query, variables });

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
                            {
                                Content = new StringContent(body, Encoding.UTF8, "application/json")
                            };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwaitFalse();

        if ((int)response.StatusCode >= 500)
        {
            throw new GatewayResponseException(response.StatusCode, response.Content?.Headers.ContentType?.MediaType, "server error");
        }

        var text = await response.ReadJsonStringAsync().ConfigureAwaitFalse();

        if (JToken.Parse(text) is not JObject json)
        {
            throw new GatewayResponseException(response.StatusCode, response.Content?.Headers.ContentType?.MediaType, "not a JSON object");
        }

        // e.g. {"data":null,"errors":[{"readable_code":"UNAUTHORIZED","message":"invalid access token or data","fa_message":"..."}]}
        var error = (json["errors"] as JArray)?.OfType<JObject>().FirstOrDefault();

        if (error != null || json["data"] is not JObject data)
        {
            var message = new[] { (string?)error?["fa_message"], (string?)error?["message"], (string?)error?["readable_code"] }
                         .FirstOrDefault(text => !text.IsNullOrWhiteSpace());

            return new GraphQlResponse(null, message ?? $"ZarinPal GraphQL request failed. HTTP status {(int)response.StatusCode}.");
        }

        return new GraphQlResponse(data, null);
    }
}

internal sealed class GraphQlResponse(JObject? data, string? error)
{
    public JObject? Data { get; } = data;

    public string? Error { get; } = error;
}
