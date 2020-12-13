namespace Parbad.Gateway.Melli
{
    public class MelliGatewayOptions
    {
        public string PaymentPageUrl { get; set; } = "https://sadad.shaparak.ir/VPG/Purchase";

        public string ApiRequestUrl { get; set; } = "https://sadad.shaparak.ir/VPG/api/v0/Request/PaymentRequest";

        public string ApiVerificationUrl { get; set; } = "https://sadad.shaparak.ir/VPG/api/v0/Advice/Verify";
    }
}
