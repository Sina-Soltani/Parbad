namespace Parbad.Gateway.IdPay.Internal
{
    internal class IdPayCallbackResult
    {
        public string Id { get; set; }

        public bool IsSucceed { get; set; }

        public IPaymentVerifyResult Result { get; set; }
    }
}