// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Gateway.Sepehr.Internal;

namespace Parbad.Gateway.Sepehr
{
    public static class SepehrGatewayPaymentResultExtensions
    {
        public static SepehrGatewayVerificationAdditionalData GetSepehrAdditionalData(this IPaymentVerifyResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            result.AdditionalData.TryGetValue(SepehrHelper.VerificationAdditionalDataKey, out var additionalData);

            return additionalData as SepehrGatewayVerificationAdditionalData;
        }

        internal static void SetPayIrAdditionalData(this IPaymentVerifyResult result, SepehrGatewayVerificationAdditionalData additionalData)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (additionalData == null) throw new ArgumentNullException(nameof(additionalData));

            result.AdditionalData.Add(SepehrHelper.VerificationAdditionalDataKey, additionalData);
        }
    }
}
