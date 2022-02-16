namespace Parbad.Gateway.Zibal
{
    public class ZibalGatewayOptions
    {
        public string RequestURl { get; set; } = "https://gateway.zibal.ir/v1/request";
        public string VerifyURl { get; set; } = "https://gateway.zibal.ir/v1/verify";
        public string PaymentUrl { get; set; } = "https://gateway.zibal.ir/start";
    }
}