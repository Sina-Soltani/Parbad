// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Pasargad.Api.Models;

public class PasargadPurchaseResponseModel
{
    public string ResultMsg { get; set; }

    public int ResultCode { get; set; }

    public PasargadPurchaseDataResponseModel Data { get; set; }
}

public class PasargadPurchaseDataResponseModel
{
    public string UrlId { get; set; }

    public string Url { get; set; }
}
