namespace Parbad.Gateway.Sepehr
{
    public class SepehrGatewayOptions
    {
        public string ApiTokenUrl = "https://sepehr.shaparak.ir:8081/V1/PeymentApi/GetToken";

        public string ApiAdviceUrl = "https://sepehr.shaparak.ir:8081/V1/PeymentApi/Advice";

        public string ApiRollbackUrl = "https://sepehr.shaparak.ir:8081/V1/PeymentApi/Rollback";

        public string PaymentPageUrl = "https://sepehr.shaparak.ir:8080/Pay";
    }
}
