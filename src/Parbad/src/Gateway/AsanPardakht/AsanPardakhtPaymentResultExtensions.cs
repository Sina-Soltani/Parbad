using Parbad.Gateway.AsanPardakht.Internal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parbad.Gateway.AsanPardakht
{
    public static class AsanPardakhtPaymentResultExtensions
    {
        private const string OriginalPaymentResultKey = "AsanPardakhtOriginalPaymentResultKey";

        /// <summary>
        /// Gets the original payment result that is received from the Asan Pardakht gateway.
        /// </summary>
        public static AsanPardakhtPaymentResultModel GetAsanPardakhtOriginalPaymentResult(this IPaymentFetchResult result)
        {
            return GetAsanPardakhtOriginalPaymentResult(result);
        }

        /// <summary>
        /// Gets the original payment result that is received from the Asan Pardakht gateway.
        /// </summary>
        public static AsanPardakhtPaymentResultModel GetAsanPardakhtOriginalPaymentResult(this IPaymentVerifyResult result)
        {
            return GetAsanPardakhtOriginalPaymentResult(result);
        }

        internal static AsanPardakhtPaymentResultModel GetAsanPardakhtOriginalPaymentResult(this IPaymentResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            if (!result.AdditionalData.ContainsKey(OriginalPaymentResultKey))
            {
                return null;
            }

            return (AsanPardakhtPaymentResultModel)result.AdditionalData[OriginalPaymentResultKey];
        }

        internal static void SetAsanPardakhtOriginalPaymentResult(this IPaymentResult result, AsanPardakhtPaymentResultModel model)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            result.AdditionalData.Add(OriginalPaymentResultKey, model);
        }
    }
}
