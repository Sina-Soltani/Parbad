// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Parbad.Internal
{
    public class PaymentRequestResult : PaymentResult, IPaymentRequestResult
    {
        /// <inheritdoc />
        public PaymentRequestResultStatus Status { get; set; }

        /// <inheritdoc />
        public IGatewayTransporter GatewayTransporter { get; set; }

        public override bool IsSucceed => Status == PaymentRequestResultStatus.Succeed;

        /// <summary>
        /// Creates an instance of <see cref="PaymentRequestResult"/> which indicates a successful result and a Gateway Transporter with type of Post.
        /// </summary>
        /// <param name="gatewayAccountName"></param>
        /// <param name="httpContext"></param>
        /// <param name="url">Page URL.</param>
        /// <param name="form">Form to post.</param>
        public static PaymentRequestResult SucceedWithPost(
            string gatewayAccountName,
            HttpContext httpContext,
            string url,
            IEnumerable<KeyValuePair<string, string>> form)
        {
            var descriptor = GatewayTransporterDescriptor.CreatePost(url, form);

            var transporter = new DefaultGatewayTransporter(httpContext, descriptor);

            return Succeed(transporter, gatewayAccountName);
        }

        /// <summary>
        /// Creates an instance of <see cref="PaymentRequestResult"/> which indicates a successful result and a Gateway Transporter with type of Redirect.
        /// </summary>
        /// <param name="gatewayAccountName"></param>
        /// <param name="httpContext"></param>
        /// <param name="url">Page URL.</param>
        public static PaymentRequestResult SucceedWithRedirect(
            string gatewayAccountName,
            HttpContext httpContext,
            string url)
        {
            var descriptor = GatewayTransporterDescriptor.CreateRedirect(url);

            var transporter = new DefaultGatewayTransporter(httpContext, descriptor);

            return Succeed(transporter, gatewayAccountName);
        }

        public static PaymentRequestResult Succeed(IGatewayTransporter gatewayTransporter, string gatewayAccountName)
        {
            return new PaymentRequestResult
            {
                GatewayAccountName = gatewayAccountName,
                GatewayTransporter = gatewayTransporter,
                Status = PaymentRequestResultStatus.Succeed
            };
        }

        public static PaymentRequestResult Failed(string message, string gatewayAccountName = null)
        {
            return new PaymentRequestResult
            {
                Status = PaymentRequestResultStatus.Failed,
                Message = message,
                GatewayAccountName = gatewayAccountName,
                GatewayTransporter = new NullGatewayTransporter()
            };
        }
    }
}
