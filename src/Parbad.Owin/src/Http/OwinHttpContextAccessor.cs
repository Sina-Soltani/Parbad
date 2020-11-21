// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Owin;
using Microsoft.Owin;

namespace Parbad.Owin.Http
{
    public class OwinHttpContextAccessor : IHttpContextAccessor
    {
        public OwinHttpContextAccessor(IOwinContext owinContext)
        {
            if (owinContext == null) throw new ArgumentNullException(nameof(owinContext));

            HttpContext = new DefaultHttpContext(new FeatureCollection(new OwinFeatureCollection(owinContext.Environment)));
        }

        public HttpContext HttpContext { get; set; }
    }
}
