// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Options;

namespace Parbad.Gateway.Saman.Internal.ResultTranslators;

internal static class SamanResultTranslator
{
    public static string Translate(string result, MessagesOptions options)
    {
        return result switch
        {
            "0" => options.PaymentSucceed,
            "2" => "درخواست تکراری می باشد",
            "-2" => "تراکنش یافته نشد",
            "-6" => "بیش از نیم ساعت از زمان اجرای تراکنش گذشته است.",
            "-104" => "ترمینال ارسال ی غیرفعا ل م ی باشد",
            "-105" => "کد پذیرنده و یا کلمه عبور آن اشتباه است",
            "-106" => "آدرس آی پ ی درخواست ی غی ر مجاز می باشد",
            _ => $"{options.UnexpectedErrorText} Response: {result}"
        };
    }
}
