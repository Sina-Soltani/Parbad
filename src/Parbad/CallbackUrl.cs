// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Exceptions;
using Parbad.Utilities;

namespace Parbad
{
    /// <summary>
    /// A complete URL of your website. It will be used by the gateway for redirecting
    /// the client again to your website.
    /// <para>Note: A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
    /// <exception cref="CallbackUrlFormatException"></exception>
    /// </summary>
    public class CallbackUrl : IComparable<CallbackUrl>
    {
        private string _url;

        /// <summary>
        /// Initializes an instance of <see cref="CallbackUrl"/> class.
        /// </summary>
        /// <param name="url">
        /// A complete URL of your website. It will be used by the gateway for redirecting
        /// the client again to your website.
        /// <para>A complete URL would be like: "http://www.mywebsite.com/foo/bar/"</para>
        /// </param>
        /// <exception cref="CallbackUrlFormatException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public CallbackUrl(string url)
        {
            if (url == null) throw new ArgumentNullException(nameof(url));

            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                throw new CallbackUrlFormatException(url);
            }

            _url = url;
        }

        /// <summary>
        /// Adds a query string to the current URL.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddQueryString(string name, string value)
        {
            _url = ParbadUrlHelper.AddQueryString(_url, name, value);
        }

        public int CompareTo(CallbackUrl other)
        {
            return string.Compare(_url, other?._url, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(CallbackUrl other)
        {
            return string.Equals(_url, other?._url, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is CallbackUrl other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _url.GetHashCode();
        }

        public override string ToString()
        {
            return _url;
        }

        public static implicit operator string(CallbackUrl callbackUrl)
        {
            if (callbackUrl == null) throw new ArgumentNullException(nameof(callbackUrl));

            return callbackUrl.ToString();
        }
    }
}
