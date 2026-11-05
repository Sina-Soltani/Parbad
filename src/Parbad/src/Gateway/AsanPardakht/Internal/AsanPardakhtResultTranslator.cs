// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;
using System.Collections.Generic;

namespace Parbad.Gateway.AsanPardakht.Internal
{
    internal enum AsanPardakhtOperation
    {
        Token,
        TranResult,
        Verify,
        Settlement
    }

    /// <summary>
    /// AsanPardakht's REST API answers with HTTP status codes, and their meaning depends on the operation (guide v1.6,
    /// p.11). The guide refers to the online Swagger for the tables, which could not be read; the per-operation
    /// meanings below follow mrdaghestani/AsanPardakht.IPG, by a co-author of the guide.
    /// Assumption (unverified against the Swagger, ANALYSIS-02 §7.3).
    /// </summary>
    internal static class AsanPardakhtResultTranslator
    {
        public static string Translate(AsanPardakhtOperation operation, AsanPardakhtResponse response, MessagesOptions options)
        {
            if (OperationMessages.TryGetValue(operation, out var messages) && messages.TryGetValue(response.StatusCode, out var message))
            {
                return message;
            }

            return CommonMessages.TryGetValue(response.StatusCode, out var commonMessage)
                ? commonMessage
                : $"{options.UnexpectedErrorText} Response: {response.StatusCode}";
        }

        /// <summary>The bank says the transaction was verified before.</summary>
        public static bool IsAlreadyVerified(AsanPardakhtResponse response) => AlreadyVerifiedCodes.Contains(response.StatusCode);

        /// <summary>The bank says the transaction was settled before.</summary>
        public static bool IsAlreadySettled(AsanPardakhtResponse response) => AlreadySettledCodes.Contains(response.StatusCode);

        private static readonly HashSet<int> AlreadyVerifiedCodes = new HashSet<int> { 472, 473, 475 };

        private static readonly HashSet<int> AlreadySettledCodes = new HashSet<int> { 474, 476 };

        private static readonly IDictionary<int, string> CommonMessages = new Dictionary<int, string>
        {
            { 400, "درخواست نامعتبر است (Bad request)" },
            { 401, "سرآیندهای احراز هویت نادرست است یا ارسال نشده است (Wrong or missing headers)" },
            { 471, "هویت درخواست کننده معتبر نیست (Identity not trusted)" },
            { 472, "رکوردی یافت نشد (No record found)" },
            { 473, "نام کاربری یا رمز عبور پذیرنده نامعتبر است (Invalid merchant user name or password)" },
            { 474, "آی پی درخواست کننده نامعتبر است (Invalid requesting IP)" },
            { 475, "شناسه فاکتور باید عدد باشد (The invoice ID must be a number)" },
            { 476, "مبلغ باید عدد باشد (The amount must be a number)" },
            { 477, "تاریخ محلی نامعتبر است (Invalid local date)" },
            { 478, "زمان محلی نامعتبر است (Invalid local time)" },
            { 485, "تاریخ و زمان محلی نامعتبر است (Invalid local date and time)" },
            { 486, "مبلغ خارج از محدوده مجاز است (Amount out of range)" },
            { 489, "شناسه فاکتور تکراری است (Duplicate invoice ID)" },
            { 490, "پذیرنده غیرفعال است (Merchant disabled)" }
        };

        private static readonly IDictionary<AsanPardakhtOperation, IDictionary<int, string>> OperationMessages =
            new Dictionary<AsanPardakhtOperation, IDictionary<int, string>>
            {
                [AsanPardakhtOperation.TranResult] = new Dictionary<int, string>
                {
                    { 471, "هویت درخواست کننده معتبر نیست (Identity not trusted)" },
                    { 472, "تراکنش یافت نشد یا توسط کاربر لغو شده است (No transaction found, or it was cancelled by the user)" }
                },
                [AsanPardakhtOperation.Verify] = new Dictionary<int, string>
                {
                    { 471, "تراکنش یافت نشد یا برگشت خورده است (Transaction not found or reversed)" },
                    { 472, "این تراکنش قبلاً وریفای شده است (Already verified)" },
                    { 473, "این تراکنش قبلاً وریفای شده است (Already verified)" },
                    { 474, "تراکنش یافت نشد یا برگشت خورده است (Transaction not found or reversed)" },
                    { 475, "این تراکنش قبلاً وریفای شده است (Already verified)" },
                    { 476, "تراکنش یافت نشد یا برگشت خورده است (Transaction not found or reversed)" },
                    { 477, "هویت یا آی پی درخواست کننده نامعتبر است (Invalid identity or IP)" }
                },
                [AsanPardakhtOperation.Settlement] = new Dictionary<int, string>
                {
                    { 471, "تراکنش یافت نشد (Transaction not found)" },
                    { 473, "تراکنش یافت نشد (Transaction not found)" },
                    { 474, "این تراکنش قبلاً تسویه شده است (Already settled)" },
                    { 475, "تراکنش یافت نشد (Transaction not found)" },
                    { 476, "این تراکنش قبلاً تسویه شده است (Already settled)" },
                    { 477, "هویت درخواست کننده نامعتبر است (Invalid identity)" }
                }
            };
    }
}
