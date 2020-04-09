// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Gateway.PayIr.Internal;

namespace Parbad.Gateway.PayIr
{
    public static class PayIrVerifyAdditionalDataExtensions
    {
        public static PayIrVerifyAdditionalData GetPayIrAdditionalData(this IPaymentVerifyResult result)
        {
            var additionalData = new PayIrVerifyAdditionalData();

            if (result.AdditionalData.TryGetValue(nameof(PayIrVerifyResponseModel.CardNumber), out var cardNumber))
            {
                additionalData.CardNumber = cardNumber;
            }

            if (result.AdditionalData.TryGetValue(nameof(PayIrVerifyResponseModel.Description), out var description))
            {
                additionalData.Description = description;
            }

            if (result.AdditionalData.TryGetValue(nameof(PayIrVerifyResponseModel.FactorNumber), out var factorNumber))
            {
                additionalData.FactorNumber = factorNumber;
            }

            if (result.AdditionalData.TryGetValue(nameof(PayIrVerifyResponseModel.Mobile), out var mobile))
            {
                additionalData.Mobile = mobile;
            }

            return additionalData;
        }
    }
}
