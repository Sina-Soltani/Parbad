// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Gateway.Saman.Internal;

namespace Parbad.Gateway.Saman;

public static class SamanPaymentResultExtensions
{
    /// <summary>
    /// Gets the additional data which is received from Saman Gateway. 
    /// </summary>
    public static SamanVerificationAdditionalData GetSamanAdditionalData(this IPaymentFetchResult paymentFetchResult) => GetAdditionalData(paymentFetchResult);

    /// <summary>
    /// Gets the additional data which is received from Saman Gateway. 
    /// </summary>
    public static SamanVerificationAdditionalData GetSamanAdditionalData(this IPaymentVerifyResult paymentVerifyResult) => GetAdditionalData(paymentVerifyResult);

    internal static void AddSamanAdditionalData(this IPaymentFetchResult paymentFetchResult, SamanVerificationAdditionalData additionalData)
    {
        if (paymentFetchResult == null) throw new ArgumentNullException(nameof(paymentFetchResult));

        paymentFetchResult.AdditionalData.Add(Constants.AdditionalVerificationDataKey, additionalData);
    }

    internal static void AddSamanAdditionalData(this IPaymentVerifyResult paymentVerifyResult, SamanVerificationAdditionalData additionalData)
    {
        if (paymentVerifyResult == null) throw new ArgumentNullException(nameof(paymentVerifyResult));

        paymentVerifyResult.AdditionalData.Add(Constants.AdditionalVerificationDataKey, additionalData);
    }

    private static SamanVerificationAdditionalData GetAdditionalData(this IPaymentResult paymentResult)
    {
        if (paymentResult == null) throw new ArgumentNullException(nameof(paymentResult));

        if (!paymentResult.AdditionalData.TryGetValue(Constants.AdditionalVerificationDataKey, out var additionalData))
        {
            return null;
        }

        return (SamanVerificationAdditionalData)additionalData;
    }
}
