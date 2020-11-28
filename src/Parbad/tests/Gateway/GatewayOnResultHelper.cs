using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parbad.Abstraction;

namespace Parbad.Tests.Gateway
{
    public static class GatewayOnResultHelper
    {
        public static void OnFetchResult(IPaymentFetchResult result, long expectedTrackingNumber, decimal expectedAmount, string expectedGatewayName)
        {
            Assert.IsNotNull(result);

            Assert.IsTrue(result.IsSucceed);
            Assert.AreEqual(PaymentFetchResultStatus.ReadyForVerifying, result.Status);

            Assert.AreEqual(expectedTrackingNumber, result.TrackingNumber);

            Assert.AreEqual(expectedGatewayName, result.GatewayName);

            Assert.AreEqual(GatewayAccount.DefaultName, result.GatewayAccountName);

            Assert.AreEqual(result.Amount, expectedAmount);

            Assert.IsFalse(result.IsAlreadyVerified);
        }

        public static void OnVerifyResult(IPaymentVerifyResult result, long expectedTrackingNumber, decimal expectedAmount, string expectedGatewayName)
        {
            Assert.IsNotNull(result);

            Assert.IsTrue(result.IsSucceed);
            Assert.AreEqual(PaymentVerifyResultStatus.Succeed, result.Status);

            Assert.AreEqual(expectedTrackingNumber, result.TrackingNumber);

            Assert.AreEqual(expectedGatewayName, result.GatewayName);

            Assert.AreEqual(GatewayAccount.DefaultName, result.GatewayAccountName);

            Assert.AreEqual(result.Amount, expectedAmount);

            Assert.IsNotNull(result.TransactionCode);
        }
    }
}
