namespace Parbad.Gateway.Zibal
{
    public class ZibalGatewayOptions
    {
        public string RequestURl => "https://gateway.zibal.ir/v1/request";
        public string VerifyURl => "https://gateway.zibal.ir/v1/verify";
        public string PaymentUrl(long trackId) => $"https://gateway.zibal.ir/start/{trackId}";
    }
}