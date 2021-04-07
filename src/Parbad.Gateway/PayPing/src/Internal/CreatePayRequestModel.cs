// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.PayPing.Internal
{
    internal class CreatePayRequestModel
    {
        public long Amount { get; set; }

        public string PayerIdentity { get; set; }

        public string PayerName { get; set; }

        public string Description { get; set; }

        public string ClientRefId { get; set; }

        public string ReturnUrl { get; set; }
    }
}
