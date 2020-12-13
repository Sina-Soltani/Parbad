namespace Parbad.Gateway.Mellat
{
    public class MellatGatewayOptions
    {
        public string PaymentPageUrl { get; set; } = "https://bpm.shaparak.ir/pgwchannel/startpay.mellat";

        public string ApiUrl { get; set; } = "https://bpm.shaparak.ir/pgwchannel/services/pgw";

        public string ApiTestUrl { get; set; } = "https://bpm.shaparak.ir/pgwchannel/services/pgwtest";
    }
}
