namespace Parbad.Gateway.AsanPardakht
{
    public class AsanPardakhtGatewayOptions
    {
        public string PaymentPageUrl { get; set; } = "https://asan.shaparak.ir/";

        public string ApiUrl { get; set; } = "https://ipgsoap.asanpardakht.ir/paygate/merchantservices.asmx";
        public string EncryptUrl { get; set; } = "https://ipgsoap.asanpardakht.ir/paygate/internalutils.asmx?op=EncryptInAES";
        public string DecryptUrl { get; set; } = "https://ipgsoap.asanpardakht.ir/paygate/internalutils.asmx?op=DecryptInAES";
    }
}