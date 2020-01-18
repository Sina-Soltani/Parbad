// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.PayIr.Internal
{
    internal class PayIrRequestModel
    {
        public string Api { get; set; }

        public string Redirect { get; set; }

        public long Amount { get; set; }
    }
}
