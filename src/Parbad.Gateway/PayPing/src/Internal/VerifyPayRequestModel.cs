// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.PayPing.Internal
{
    public class VerifyPayRequestModel
    {
        public long Amount { get; set; }

        public string RefId { get; set; }
    }
}
