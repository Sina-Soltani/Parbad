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

        paymentFetchResult.AdditionalData.Add(SamanHelper.AdditionalVerificationDataKey, additionalData);
    }

    internal static void AddSamanAdditionalData(this IPaymentVerifyResult paymentVerifyResult, SamanVerificationAdditionalData additionalData)
    {
        if (paymentVerifyResult == null) throw new ArgumentNullException(nameof(paymentVerifyResult));

        paymentVerifyResult.AdditionalData.Add(SamanHelper.AdditionalVerificationDataKey, additionalData);
    }

    private static SamanVerificationAdditionalData GetAdditionalData(this IPaymentResult paymentResult)
    {
        if (paymentResult == null) throw new ArgumentNullException(nameof(paymentResult));

        if (!paymentResult.AdditionalData.ContainsKey(SamanHelper.AdditionalVerificationDataKey))
        {
            return null;
        }

        return (SamanVerificationAdditionalData)paymentResult.AdditionalData[SamanHelper.AdditionalVerificationDataKey];
    }
}
