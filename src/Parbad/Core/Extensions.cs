using System;

namespace Parbad.Core
{
    internal static class Extensions
    {
        public static bool IsSuccess(this RequestResult requestResult)
        {
            if (requestResult == null)
            {
                throw new ArgumentNullException(nameof(requestResult));
            }

            return requestResult.Status == RequestResultStatus.Success;
        }

        public static bool IsSuccess(this VerifyResult verifyResult)
        {
            if (verifyResult == null)
            {
                throw new ArgumentNullException(nameof(verifyResult));
            }

            return verifyResult.Status == VerifyResultStatus.Success;
        }

        public static bool IsSuccess(this RefundResult refundResult)
        {
            if (refundResult == null)
            {
                throw new ArgumentNullException(nameof(refundResult));
            }

            return refundResult.Status == RefundResultStatus.Success;
        }
    }
}