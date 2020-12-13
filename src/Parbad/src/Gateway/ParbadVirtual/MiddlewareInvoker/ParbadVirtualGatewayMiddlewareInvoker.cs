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

namespace Parbad.Gateway.ParbadVirtual.MiddlewareInvoker
{
    internal class ParbadVirtualGatewayMiddlewareInvoker : IParbadVirtualGatewayMiddlewareInvoker
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptions<ParbadVirtualGatewayOptions> _options;

        public ParbadVirtualGatewayMiddlewareInvoker(IOptions<ParbadVirtualGatewayOptions> options, IHttpContextAccessor httpContextAccessor)
        {
            _options = options;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task InvokeAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            HttpResponseUtilities.AddNecessaryContents(httpContext, "text/html");

            var commandDetails = await GetCommandDetails(httpContext);

            if (commandDetails == null)
            {
                await httpContext.Response.WriteAsync("Command details are not valid.");
                return;
            }

            switch (commandDetails.CommandType)
            {
                case VirtualGatewayCommandType.Request:
                    await HandleRequestPage(httpContext, commandDetails);
                    break;

                case VirtualGatewayCommandType.Pay:
                    await HandleResultPage(httpContext, commandDetails);
                    break;

                default:
                    await httpContext.Response.WriteAsync("CommandType is not valid.");
                    break;
            }
        }

        private async Task HandleRequestPage(HttpContext httpContext, VirtualGatewayCommandDetails commandDetails)
        {
            var template = await GetTemplate("VirtualGatewayRequestTemplate.html");

            var html = template
                .Replace("#VirtualGatewayPath#", _options.Value.GatewayPath)
                .Replace("#TrackingNumber#", commandDetails.TrackingNumber.ToString())
                .Replace("#Amount#", commandDetails.Amount.ToString())
                .Replace("#DisplayAmount#", ((long)commandDetails.Amount).ToString("N0"))
                .Replace("#RedirectUrl#", commandDetails.RedirectUrl)
                .Replace("#YearNow#", DateTime.Now.Year.ToString());

            await httpContext.Response.WriteAsync(html);
        }

        private static async Task HandleResultPage(HttpContext httpContext, VirtualGatewayCommandDetails commandDetails)
        {
            if (!httpContext.Request.Form.TryGetValue("isPaid", out var isPaid) ||
                !bool.TryParse(isPaid, out var boolIsPaid))
            {
                await httpContext.Response.WriteAsync($"{nameof(isPaid)} field is not valid.");
                return;
            }

            var template = await GetTemplate("VirtualGatewayResultTemplate.html");

            var html = template
                .Replace("#TrackingNumber#", commandDetails.TrackingNumber.ToString())
                .Replace("#DisplayAmount#", ((long)commandDetails.Amount).ToString("N0"))
                .Replace("#TransactionCode#", boolIsPaid ? Guid.NewGuid().ToString("N") : string.Empty)
                .Replace("#RedirectUrl#", commandDetails.RedirectUrl)
                .Replace("#IsPaid#", isPaid.ToString().ToLower())
                .Replace("#YearNow#", DateTime.Now.Year.ToString())
                .Replace("#CssStatusName#", boolIsPaid ? "success" : "danger")
                .Replace("#StatusText#", boolIsPaid ? "Succeed" : "Failed");

            await httpContext.Response.WriteAsync(html);
        }

        private static async Task<VirtualGatewayCommandDetails> GetCommandDetails(HttpContext httpContext)
        {
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
    }
}
