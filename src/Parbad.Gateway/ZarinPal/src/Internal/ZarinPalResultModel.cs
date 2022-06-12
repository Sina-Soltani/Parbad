// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad.Gateway.ZarinPal.Internal
{
    public class ZarinPalResultModel<TData> where TData : class
    {
        public TData Data { get; set; }

        public List<string> Errors { get; set; }
    }
}
