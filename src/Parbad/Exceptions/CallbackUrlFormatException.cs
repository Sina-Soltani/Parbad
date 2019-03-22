// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Exceptions
{
    [Serializable]
    public class CallbackUrlFormatException : Exception
    {
        public CallbackUrlFormatException(string url) : base($"The format of the given URL: {url} is not valid. A valid URL would be like: http(s)://www.site.com/foo/bar")
        {
        }
    }
}
