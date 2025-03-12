// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Pasargad.Internal.Models;

internal class PasargadCallbackResultModel
{
    /// <summary>
    /// شماره فاکتوری که در مرحله ثبت خرید، شارژ یا پرداخت قبض توسط پذیرنده ارسال شده است
    /// </summary>
    public string InvoiceId { get; set; }

    /// <summary>
    /// وضعیت پرداخت
    /// </summary>
    public PasargadCallbackStatusResult Status { get; set; }

    /// <summary>
    /// شماره ارجاع شاپرکی
    /// </summary>
    public string ReferenceNumber { get; set; }

    /// <summary>
    /// کد پیگیری
    /// </summary>
    public string TrackId { get; set; }
}

public enum PasargadCallbackStatusResult
{
    /// <summary>
    /// پرداخت موفق
    /// </summary>
    Success,

    /// <summary>
    /// پرداخت ناموفق
    /// </summary>
    Failed,

    /// <summary>
    /// نامشخص
    /// </summary>
    Unknown
}