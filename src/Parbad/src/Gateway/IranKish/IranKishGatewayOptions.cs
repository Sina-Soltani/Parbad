namespace Parbad.Gateway.IranKish
{
    public class IranKishGatewayOptions
    {
        public string PaymentPageUrl { get; set; } = "https://ikc.shaparak.ir/TPayment/Payment/index";

        public string ApiTokenUrl { get; set; } = "https://ikc.shaparak.ir/TToken/Tokens.svc";

        public string ApiVerificationUrl { get; set; } = "https://ikc.shaparak.ir/TVerify/Verify.svc";
    }
}
