// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad
{
    /// <summary>
    /// Describes a gateway transporter.
    /// </summary>
    public class GatewayTransporterDescriptor
    {
        public TransportType Type { get; set; }

        public string Url { get; set; }

        public IEnumerable<KeyValuePair<string, string>> Form { get; set; }

        public static GatewayTransporterDescriptor CreateRedirect(string url)
        {
            return new GatewayTransporterDescriptor
            {
                Type = TransportType.Redirect,
                Url = url,
                Form = null
            };
        }

        public static GatewayTransporterDescriptor CreatePost(string url, IEnumerable<KeyValuePair<string, string>> form)
        {
            return new GatewayTransporterDescriptor
            {
                Type = TransportType.Post,
                Url = url,
                Form = form
            };
        }

        public enum TransportType
        {
            Post,
            Redirect
        }
    }
}
