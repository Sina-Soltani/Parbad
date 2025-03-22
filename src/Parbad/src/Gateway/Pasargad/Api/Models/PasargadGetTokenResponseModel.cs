// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Pasargad.Api.Models;

public class PasargadGetTokenResponseModel
{
    public int ResultCode { get; set; }

    public string ResultMsg { get; set; }

    public string Token { get; set; }

    public string Username { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string BusinessName { get; set; }
}
