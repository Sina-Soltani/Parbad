using System;
using System.Text;
using System.Web;
using Parbad.Configurations;
using Parbad.Core;
using Parbad.Providers.Parbad;
using Parbad.Utilities;

namespace Parbad.Web.Gateway
{
    /// <summary>
    /// Parbad Virtual Gateway HttpHandler to test website's functionality with a virtual payment.
    /// </summary>
    public sealed class ParbadVirtualGatewayHandler : IHttpHandler
    {
        private static ParbadVirtualGatewayConfiguration GatewayConfiguration => ParbadConfiguration.Gateways.GetParbadVirtualGatewayConfiguration();

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext), "HttpContext is null. It's required for ParbadVirtualGatewayHandler");
            }

            httpContext.Response.AppendParbadHeaders();

            var commandDetails = GetCommandDetails(httpContext);

            if (commandDetails == null)
            {
                WriteToResponse(httpContext, "command details are not valid.");
                return;
            }

            switch (commandDetails.CommandType)
            {
                case GatewayCommandType.Request:
                    HandleRequestCommand(httpContext, commandDetails);
                    break;

                case GatewayCommandType.Pay:
                    HandlePayCommand(httpContext, commandDetails);
                    break;

                default:
                    WriteToResponse(httpContext, "CommandType is not valid.");
                    break;
            }

            httpContext.Response.ContentType = "text/html";
            httpContext.Response.ContentEncoding = Encoding.UTF8;

            ResponseCompressor.Compress(httpContext);
        }

        private static CommandDetails GetCommandDetails(HttpContext context)
        {
            if (!long.TryParse(context.Request.Form["orderNumber"], out var orderNumber))
            {
                return null;
            }

            var redirectUrl = context.Request.Form["redirectUrl"];

            if (!Money.TryParse(context.Request.Form["amount"], out var amount))
            {
                return null;
            }

            if (redirectUrl.IsNullOrWhiteSpace())
            {
                return null;
            }

            if (!Enum.TryParse(context.Request.Form["commandType"], out GatewayCommandType commandType))
            {
                return null;
            }

            return new CommandDetails(commandType, orderNumber, amount, redirectUrl);
        }

        private static bool IsGatewayPasswordValid(string password)
        {
            return CommonTools.HashPassword(password) == GatewayConfiguration.GatewayPassword;
        }

        private static void HandleRequestCommand(HttpContext context, CommandDetails commandDetails)
        {
            var parbadGatewayUrl = context.Request.Url.Authority + context.Request.Url.AbsolutePath;

            if (!parbadGatewayUrl.StartsWith(context.Request.Url.Scheme + "://"))
            {
                parbadGatewayUrl = $"{context.Request.Url.Scheme}://{parbadGatewayUrl}";
            }

            var html = GenerateRequestCommandHtml(commandDetails.OrderNumber, commandDetails.Amount, commandDetails.RedirectUrl, parbadGatewayUrl, GatewayConfiguration.HasPassword());

            WriteToResponse(context, html);
        }

        private static string GenerateRequestCommandHtml(long orderNumber, Money amount, string redirectUrl, string parbadGatewayUrl, bool isPasswordRequired)
        {
            return Resource.ParbadVirtualGatewayRequestHtml
                .Replace("#OrderNumber#", orderNumber.ToString())
                .Replace("#DisplayOrderNumber#", orderNumber.ToString("N0"))
                .Replace("#Amount#", amount.ToString())
                .Replace("#DisplayAmount#", amount.ToString("N0"))
                .Replace("#RedirectUrl#", redirectUrl)
                .Replace("#ParbadGatewayUrl#", parbadGatewayUrl)
                .Replace("#IsPasswordRequired#", isPasswordRequired.ToString().ToLower());
        }

        private static void HandlePayCommand(HttpContext context, CommandDetails commandDetails)
        {
            if (!bool.TryParse(context.Request.Form["isPayed"], out var isPayed))
            {
                WriteToResponse(context, "IsPayed field is not valid.");
                return;
            }

            if (isPayed && GatewayConfiguration.HasPassword())
            {
                var password = context.Request.Form["password"];

                if (password.IsNullOrEmpty())
                {
                    WriteToResponse(context, "Password is required.");
                    return;
                }

                if (!IsGatewayPasswordValid(password))
                {
                    WriteToResponse(context, "Password is not correct.");
                    return;
                }
            }

            var html = GeneratePayCommandHtml(commandDetails.OrderNumber, commandDetails.OrderNumber.ToString(), commandDetails.Amount, commandDetails.RedirectUrl, isPayed);

            WriteToResponse(context, html);
        }

        private static string GeneratePayCommandHtml(long orderNumber, string referenceId, Money amount, string redirectUrl, bool isPayed)
        {
            return Resource.ParbadVirtualGatewayResultHtml
                .Replace("#DisplayOrderNumber#", orderNumber.ToString("N0"))
                .Replace("#DisplayAmount#", amount.ToString("N0"))
                .Replace("#ReferenceId#", referenceId)
                .Replace("#TransactionId#", isPayed ? Guid.NewGuid().ToString("N") : string.Empty)
                .Replace("#RedirectUrl#", redirectUrl)
                .Replace("#IsPayed#", isPayed.ToString().ToLower());
        }

        private static void WriteToResponse(HttpContext context, string text)
        {
            context.Response.Write(text);
        }
    }
}