// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.AsanPardakht.Internal.Models
{
    internal class AsanPardakhtApiErrorModel
    {
        public string Type { get; set; }

        public string Title { get; set; }

        public int Status { get; set; }

        public string Detail { get; set; }

        public string Instance { get; set; }
    }
}
