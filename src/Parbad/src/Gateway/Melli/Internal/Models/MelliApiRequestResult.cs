// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Gateway.Melli.Internal.Models
{
    [Serializable]
    internal class MelliApiRequestResult
    {
        public int? ResCode { get; set; }

        public string Token { get; set; }

        public string Description { get; set; }
    }
}