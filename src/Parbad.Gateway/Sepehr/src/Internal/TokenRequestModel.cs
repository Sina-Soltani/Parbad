// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Sepehr.Internal
{
    internal class TokenRequestModel
    {
        public long Amount { get; set; }

        public string CallbackUrl { get; set; }

        public string InvoiceId { get; set; }

        public long TerminalId { get; set; }
    }
}