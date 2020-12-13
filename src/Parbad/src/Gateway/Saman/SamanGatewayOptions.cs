namespace Parbad.Gateway.Saman
{
    public class SamanGatewayOptions
    {
        public string WebPaymentPageUrl { get; set; } = "https://sep.shaparak.ir/payment.aspx";

        public string WebApiUrl { get; set; } = "https://sep.shaparak.ir/payments/referencepayment.asmx";

        public string WebApiTokenUrl { get; set; } = "https://sep.shaparak.ir/payments/initpayment.asmx";

        public string MobilePaymentPageUrl { get; set; } = "https://sep.shaparak.ir/OnlinePG/OnlinePG";

        public string MobileApiTokenUrl { get; set; } = "https://sep.shaparak.ir/MobilePG/MobilePayment";

        public string MobileApiVerificationUrl { get; set; } = "https://verify.sep.ir/Payments/ReferencePayment.asmx";
    }
}
