namespace Parbad.Gateway.FanAva
{
    public class FanAvaGatewayOptions
    {
        public string ApiTokenGenerationUrl { get; set; } = "https://fcp.shaparak.ir/ref-payment/RestServices/mts/generateTokenWithNoSign";

        public string PaymentPageUrl { get; set; } = "https://fcp.shaparak.ir/_ipgw_/payment";

        public string ApiCheckPaymentUrl { get; set; } = "https://fcp.shaparak.ir/ref-payment/RestServices/mts/inquiryMerchantToken";

        public string ApiVerificationUrl { get; set; } = "https://fcp.shaparak.ir/ref-payment/RestServices/mts/verifyMerchantTrans";

        public string ApiRefundUrl { get; set; } = "https://fcp.shaparak.ir/ref-payment/RestServices/mts/reverseMerchantTrans";
    }
}
