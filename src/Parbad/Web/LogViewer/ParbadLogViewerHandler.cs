using System;
using System.Text;
using System.Web;
using Parbad.Configurations;
using Parbad.Infrastructure.Logging;
using Parbad.Utilities;

namespace Parbad.Web.LogViewer
{
    public sealed class ParbadLogViewerHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            if (ParbadConfiguration.Logger.Provider == null)
            {
                WriteResponse(httpContext, "ParbadConfiguration.Logger.Provider is not implemented.");
                return;
            }

            if (ParbadConfiguration.Logger.LogViewerHandlerPath.IsNullOrWhiteSpace())
            {
                WriteResponse(httpContext, "ParbadConfiguration.Logger.LogViewerHandlerPath is null or empty.");
                return;
            }

            httpContext.Response.AppendParbadHeaders();

            if (!httpContext.Request.IsAjaxRequest())
            {
                WriteHtmlPage(httpContext.Response);
                return;
            }

            //  Read logs

            var paginationRequestContext = GetPaginationRequestContext(httpContext.Request);

            var responseContext =
                ParbadConfiguration.Logger.Provider.Read(paginationRequestContext.PageNumber,
                    paginationRequestContext.PageSize) ??
                LogReadContext.Empty;

            SendDataToClient(httpContext.Response, responseContext);
        }

        private static PaginationRequestContext GetPaginationRequestContext(HttpRequest httpRequest)
        {
            int.TryParse(httpRequest["pageNumber"], out var pageNumber);

            int.TryParse(httpRequest["pageSize"], out var pageSize);

            return new PaginationRequestContext(pageNumber, pageSize);
        }

        private static void WriteHtmlPage(HttpResponse httpResponse)
        {
            httpResponse.ContentType = "text/html";
            httpResponse.ContentEncoding = Encoding.UTF8;

            var html = Resource.LogViewerHtml;

            html = html.Replace("#URL#", ParbadConfiguration.Logger.LogViewerHandlerPath.ToggleStringAtStart("/", true));

            httpResponse.Write(html);
        }

        private static void SendDataToClient(HttpResponse httpResponse, LogReadContext responseContext)
        {
            if (httpResponse == null)
            {
                throw new ArgumentNullException(nameof(httpResponse));
            }

            if (responseContext == null)
            {
                throw new ArgumentNullException(nameof(responseContext));
            }

            httpResponse.ContentType = "application/json";
            httpResponse.ContentEncoding = Encoding.UTF8;

            var json = SerializeToJson(responseContext);

            httpResponse.Write(json);
        }

        private static void WriteResponse(HttpContext httpContext, string value)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            httpContext.Response.Write(value);
        }

        private static string SerializeToJson(LogReadContext responseContext)
        {
            var logs = "";

            foreach (var log in responseContext.Logs)
            {
                logs += "{" +
                        $"\"type\": \"{log.Type}\"," +
                        $"\"gateway\": \"{log.Gateway.ToString()}\"," +
                        $"\"orderNumber\": {log.OrderNumber}," +
                        $"\"amount\": {log.Amount}," +
                        $"\"referenceId\": \"{log.ReferenceId}\"," +
                        $"\"transactionId\": \"{log.TransactionId}\"," +
                        $"\"status\": \"{log.Status}\"," +
                        $"\"message\": \"{log.Message}\"," +
                        $"\"createdOn\": \"{log.CreatedOn:yyyy/MM/dd HH:mm:ss}\"" +
                        "},";
            }

            logs = logs.ToggleStringAtEnd(",", false);

            return "{" +
                   $"\"totalLogCount\": {responseContext.TotalLogCount}," +
                   $"\"logs\": [{logs}]" +
                   "}";
        }
    }
}