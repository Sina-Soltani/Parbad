// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Pasargad.Api.Models;

public class PasargadRefundPaymentRequestModel
{
    /// <summary>
    /// شماره فاکتور که توسط پذیرنده هنگام ثبت درخواست خرید فرستاده شده
    /// </summary>
    public string Invoice { get; set; }

    /// <summary>
    /// توکن خرید که در جواب ثبت درخواست خرید دریافت شده است
    /// </summary>
    public string UrlId { get; set; }
}
