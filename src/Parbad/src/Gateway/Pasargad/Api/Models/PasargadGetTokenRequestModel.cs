// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.Pasargad.Api.Models;

public class PasargadGetTokenRequestModel
{
    public string Username { get; set; }

    public string Password { get; set; }
}
