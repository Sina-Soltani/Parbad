// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Parbad
{
    /// <summary>
    /// Describes a gateway transporter.
    /// </summary>
    public class GatewayTransporterDescriptor
    {
        public GatewayTransporterDescriptor(TransportType type, string url, IEnumerable<KeyValuePair<string, string>> form)
        {
            Type = type;
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Form = form;
        }

        public TransportType Type { get; }

        public string Url { get; }

        public IEnumerable<KeyValuePair<string, string>> Form { get; }

        public static GatewayTransporterDescriptor CreateRedirect(string url)
        {
            return new GatewayTransporterDescriptor(TransportType.Redirect, url, null);
        }

        public static GatewayTransporterDescriptor CreatePost(string url, IEnumerable<KeyValuePair<string, string>> form)
        {
            return new GatewayTransporterDescriptor(TransportType.Post, url, form);
        }

        public enum TransportType
        {
            Post,
            Redirect
        }
    }
}
