namespace Parbad.Gateway.PayIr
{
    public class PayIrGatewayOptions
    {
        public string ApiRequestUrl { get; set; } = "https://pay.ir/pg/send";

        public string ApiVerificationUrl { get; set; } = "https://pay.ir/pg/verify";

        public string PaymentPageUrl { get; set; } = "https://pay.ir/pg/";
    }
}
