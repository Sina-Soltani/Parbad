// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Zibal.Internal
{
    internal class ZibalResponseModel
    {
        public long TrackId { get; set; }

        public int Result { get; set; }

        public string Message { get; set; }

        public string? PayLink { get; set; }
    }
}
