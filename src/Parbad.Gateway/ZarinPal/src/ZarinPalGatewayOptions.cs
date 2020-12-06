namespace Parbad.Gateway.ZarinPal
{
    public class ZarinPalGatewayOptions
    {
        public string ApiUrl { get; set; } = "https://#.zarinpal.com/pg/services/WebGate/service";

        public string PaymentPageUrl { get; set; } = "https://#.zarinpal.com/pg/StartPay/";
    }
}
