// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.YekPay.Internal
{
    internal class YekPayVerificationResponseModel
    {
        public int Code { get; set; }

        public string Description { get; set; }

        public string Reference { get; set; }
    }
}