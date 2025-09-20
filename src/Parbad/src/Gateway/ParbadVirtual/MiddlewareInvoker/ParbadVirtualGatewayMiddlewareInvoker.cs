// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Parbad.Internal;
using Parbad.Options;

namespace Parbad.Gateway.ParbadVirtual.MiddlewareInvoker;

internal class ParbadVirtualGatewayMiddlewareInvoker : IParbadVirtualGatewayMiddlewareInvoker
{
    private readonly HttpContext _httpContext;
    private readonly ParbadVirtualGatewayOptions _virtualGatewayOptions;
    private readonly ParbadOptions _options;

    public ParbadVirtualGatewayMiddlewareInvoker(IHttpContextAccessor httpContextAccessor,
                                                 IOptions<ParbadVirtualGatewayOptions> virtualGatewayOptions,
                                                 IOptions<ParbadOptions> options)
    {
        _httpContext = httpContextAccessor.HttpContext;
        _virtualGatewayOptions = virtualGatewayOptions.Value;
        _options = options.Value;
    }

    public async Task InvokeAsync()
    {
        if (_httpContext == null)
        {
            throw new InvalidOperationException("HttpContext is null");
        }

        if (_httpContext.Request.Query.ContainsKey("css"))
        {
            await HandleCss(_httpContext);

            return;
        }

        HttpResponseUtilities.AddNecessaryContents(_httpContext, "text/html");

        var commandDetails = await GetCommandDetails(_httpContext);

        if (commandDetails == null)
        {
            await _httpContext.Response.WriteAsync("Parbad Virtual Gateway message: Invalid data is received.");

            return;
        }

        var cssUrl =
            $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}{_httpContext.Request.PathBase}?css=true";

        var nonceProp = GetNonceProp();

        switch (commandDetails.CommandType)
        {
            case VirtualGatewayCommandType.Request:
                await HandleRequestPage(_httpContext, commandDetails, cssUrl, nonceProp);

                break;

            case VirtualGatewayCommandType.Pay:
                await HandleResultPage(_httpContext, commandDetails, cssUrl, nonceProp);

                break;

            default:
                await _httpContext.Response.WriteAsync("CommandType is not valid.");

                break;
        }
    }

    private async Task HandleCss(HttpContext httpContext)
    {
        HttpResponseUtilities.AddNecessaryContents(_httpContext, "text/css");

        var cssContent = await GetTemplate("ParbadVirtualGatewayStyles.css");

        await httpContext.Response.WriteAsync(cssContent);
    }

    private async Task HandleRequestPage(HttpContext httpContext,
                                         VirtualGatewayCommandDetails commandDetails,
                                         string cssUrl,
                                         string nonceProp)
    {
        var template = await GetTemplate("VirtualGatewayRequestTemplate.html");

        var html = template
                  .Replace("#CssUrl#", cssUrl)
                  .Replace("#nonce#", nonceProp)
                  .Replace("#VirtualGatewayPath#", _virtualGatewayOptions.GatewayPath)
                  .Replace("#TrackingNumber#", commandDetails.TrackingNumber.ToString())
                  .Replace("#Amount#", commandDetails.Amount.ToString())
                  .Replace("#DisplayAmount#", ((long)commandDetails.Amount).ToString("N0"))
                  .Replace("#RedirectUrl#", commandDetails.RedirectUrl)
                  .Replace("#YearNow#", DateTime.Now.Year.ToString());

        await httpContext.Response.WriteAsync(html);
    }

    private static async Task HandleResultPage(HttpContext httpContext,
                                               VirtualGatewayCommandDetails commandDetails,
                                               string cssUrl,
                                               string nonceProp)
    {
        if (!httpContext.Request.Form.TryGetValue("isPaid", out var isPaid) ||
            !bool.TryParse(isPaid, out var boolIsPaid))
        {
            await httpContext.Response.WriteAsync($"{nameof(isPaid)} field is not valid.");

            return;
        }

        var template = await GetTemplate("VirtualGatewayResultTemplate.html");

        var html = template
                  .Replace("#CssUrl#", cssUrl)
                  .Replace("#nonce#", nonceProp)
                  .Replace("#TrackingNumber#", commandDetails.TrackingNumber.ToString())
                  .Replace("#DisplayAmount#", ((long)commandDetails.Amount).ToString("N0"))
                  .Replace("#TransactionCode#", boolIsPaid ? Guid.NewGuid().ToString("N") : string.Empty)
                  .Replace("#RedirectUrl#", commandDetails.RedirectUrl)
                  .Replace("#IsPaid#", isPaid.ToString().ToLower())
                  .Replace("#YearNow#", DateTime.Now.Year.ToString())
                  .Replace("#StatusText#", boolIsPaid ? "Paid" : "Cancelled");

        await httpContext.Response.WriteAsync(html);
    }

    private static async Task<VirtualGatewayCommandDetails> GetCommandDetails(HttpContext httpContext)
    {
        if (!httpContext.Request.HasFormContentType)
        {
            return null;
        }

        var form = await httpContext.Request.ReadFormAsync();

        if (!Enum.TryParse(form["commandType"], out VirtualGatewayCommandType commandType))
        {
            commandType = VirtualGatewayCommandType.Request;
        }

        if (!long.TryParse(form["trackingNumber"], out var trackingNumber) ||
            !form.TryGetValue("redirectUrl", out var redirectUrl) ||
            !Money.TryParse(form["amount"], out var amount))
        {
            return null;
        }

        return new VirtualGatewayCommandDetails(commandType, trackingNumber, amount, redirectUrl);
    }

    private static Task<string> GetTemplate(string templateName)
    {
        using var stream = typeof(ParbadVirtualGatewayOptions)
                          .GetTypeInfo()
                          .Assembly
                          .GetManifestResourceStream($"Parbad.Gateway.ParbadVirtual.{templateName}");

        var streamReader = new StreamReader(stream, Encoding.UTF8);

        return streamReader.ReadToEndAsync();
    }

    private string GetNonceProp()
    {
        string nonceValue = null;

        if (_options.NonceFactory != null)
        {
            nonceValue = _options.NonceFactory(_httpContext);
        }

        return string.IsNullOrWhiteSpace(nonceValue) ? null : $" nonce=\"{nonceValue}\"";
    }
}
