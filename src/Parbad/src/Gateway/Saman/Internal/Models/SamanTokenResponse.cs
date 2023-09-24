// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Saman.Internal.Models;

internal class SamanTokenResponse
{
    public int Status { get; set; }

    public string Token { get; set; }

    public string ErrorCode { get; set; }

    public string ErrorDesc { get; set; }
}
