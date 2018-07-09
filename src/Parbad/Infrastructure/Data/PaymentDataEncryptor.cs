using System;
using Parbad.Utilities;

namespace Parbad.Infrastructure.Data
{
    internal static class PaymentDataEncryptor
    {
        public static string Encrypt(PaymentData paymentData)
        {
            if (paymentData == null)
            {
                throw new ArgumentNullException(nameof(paymentData));
            }

            return CommonTools.Encrypt(CommonTools.SerializeObject(paymentData));
        }

        public static PaymentData Decrypt(string encryptedValue)
        {
            if (encryptedValue == null)
            {
                throw new ArgumentNullException(nameof(encryptedValue));
            }

            return CommonTools.DeserializeObject<PaymentData>(CommonTools.Decrypt(encryptedValue));
        }

        public static bool TryDecrypt(string encryptedValue, out PaymentData paymentData)
        {
            if (encryptedValue == null)
            {
                paymentData = null;
                return false;
            }

            try
            {
                paymentData = CommonTools.DeserializeObject<PaymentData>(CommonTools.Decrypt(encryptedValue));
                return true;
            }
            catch
            {
                paymentData = null;
                return false;
            }
        }
    }
}