namespace Parbad.Gateway.Parsian
{
    public class ParsianGatewayOptions
    {
        public string PaymentPageUrl = "https://pec.shaparak.ir/NewIPG/";

        public string ApiRequestUrl = "https://pec.shaparak.ir/NewIPGServices/Sale/SaleService.asmx";

        public string ApiVerificationUrl = "https://pec.shaparak.ir/NewIPGServices/Confirm/ConfirmService.asmx";

        public string ApiRefundUrl = "https://pec.shaparak.ir/NewIPGServices/Reverse/ReversalService.asmx";
    }
}
