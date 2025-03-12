// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Pasargad.Api.Models;

public class PasargadPurchaseRequestModel
{
    /// <summary>
    /// شماره ترمینال پرداختی
    /// </summary>
    public string TerminalNumber { get; set; }

    /// <summary>
    /// شماره فاکتور یکتا
    /// </summary>
    public string Invoice { get; set; }

    /// <summary>
    /// آدرسی که پس از عملیات پرداخت، کاربر به آن صفحه منتقل می شود
    /// باید دامنه کالبک هنگام ثبت نام در دیتابیس تعریف شده باشد.
    /// </summary>
    public string CallbackApi { get; set; }

    /// <summary>
    /// تاریخ فاکتور به فرمت دلخواه
    /// </summary>
    public string InvoiceDate { get; set; }

    /// <summary>
    /// مبلغ به ریال
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// کد سرویس که برای خرید باید مقدار ثابت 8 ارسال شود
    /// </summary>
    public int ServiceCode { get; set; } = 8;

    /// <summary>
    /// کد سرویس که برای خرید باید مقدار ثابت PURCHASE ارسال شود
    /// </summary>
    public string ServiceType { get; set; } = "PURCHASE";

    /// <summary>
    /// تلفن همراه پرداخت کننده
    /// </summary>
    public string MobileNumber { get; set; }

    /// <summary>
    /// توضیحات
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// ایمیل پرداخت کننده
    /// </summary>
    public string PayerMail { get; set; }

    /// <summary>
    /// نام پرداخت کننده
    /// </summary>
    public string PayerName { get; set; }

    /// <summary>
    /// کدملی
    /// </summary>
    public string NationalCode { get; set; }

    /// <summary>
    /// شماره کارت ها
    /// </summary>
    public string Pans { get; set; }
}

