namespace Parbad.Gateway.YekPay
{
    public class YekPayGatewayOptions
    {
        public string ApiRequestUrl { get; set; } = "https://gate.yekpay.com/api/payment/request";

        public string ApiVerificationUrl { get; set; } = "https://gate.yekpay.com/api/payment/verify";

        public string PaymentPageUrl { get; set; } = "https://gate.yekpay.com/api/payment/start/";
    }
}
