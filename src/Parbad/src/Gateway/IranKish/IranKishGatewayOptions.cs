namespace Parbad.Gateway.IranKish
{
    public class IranKishGatewayOptions
    {
        public string PaymentPageUrl { get; set; } = "https://ikc.shaparak.ir/iuiv3/IPG/Index";

        public string ApiTokenUrl { get; set; } = "https://ikc.shaparak.ir/api/v3/tokenization/make";

        public string ApiVerificationUrl { get; set; } = "https://ikc.shaparak.ir/api/v3/confirmation/purchase";

        public string ApiInquiryUrl { get; set; } = "https://ikc.shaparak.ir/api/v3/inquiry/single";
    }
}
