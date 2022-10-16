// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.IranKish.Internal.Models
{
    internal class IranKishVerifyRequest
    {
        public string TerminalId { get; set; }

        public string RetrievalReferenceNumber { get; set; }

        public string SystemTraceAuditNumber { get; set; }

        public string TokenIdentity { get; set; }

    }

}
