// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Gateway.ZarinPal.Models;

namespace Parbad.Gateway.ZarinPal;

public static class ZarinPalPaymentResultExtensions
{
    private static string ZarinPalOriginalVerificationResultKey => nameof(ZarinPalOriginalVerificationResultKey);

    /// <summary>
    /// Gets the original verification result that was received from the ZarinPal gateway.
    /// </summary>
    public static ZarinPalOriginalVerificationResult GetZarinPalOriginalVerificationResult(this IPaymentVerifyResult result)
    {
        if (result.AdditionalData.TryGetValue(ZarinPalOriginalVerificationResultKey, out var value))
        {
            return (ZarinPalOriginalVerificationResult)value;
        }

        return null;
    }

    internal static void SetZarinPalOriginalVerificationResult(this IPaymentVerifyResult result, ZarinPalOriginalVerificationResult verificationResult)
    {
        result.AdditionalData.Add(ZarinPalOriginalVerificationResultKey, verificationResult);
    }
}
