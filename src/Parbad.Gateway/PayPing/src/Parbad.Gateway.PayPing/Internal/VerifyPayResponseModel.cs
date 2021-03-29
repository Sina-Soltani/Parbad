namespace Parbad.Gateway.PayPing.Internal
{
    internal class VerifyPayResponseModel
    {
        public decimal Amount { get; set; }
        public string CardNumber { get; set; }
        public string CardHashPan { get; set; }
    }
}