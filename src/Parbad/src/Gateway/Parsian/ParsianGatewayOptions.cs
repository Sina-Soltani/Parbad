namespace Parbad.Gateway.Parsian
{
    public class ParsianGatewayOptions
    {
        public string PaymentPageUrl { get; set; } = "https://pec.shaparak.ir/NewIPG/";

        public string ApiRequestUrl { get; set; } = "https://pec.shaparak.ir/NewIPGServices/Sale/SaleService.asmx";

        public string ApiVerificationUrl { get; set; } = "https://pec.shaparak.ir/NewIPGServices/Confirm/ConfirmService.asmx";

        public string ApiRefundUrl { get; set; } = "https://pec.shaparak.ir/NewIPGServices/Reverse/ReversalService.asmx";
    }
}
