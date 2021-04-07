namespace Parbad.Gateway.PayPing.Internal
{
    internal class VerifyPayResponseModel
    {
        public int Amount { get; set; }
        public string CardNumber { get; set; }
        public string CardHashPan { get; set; }
    }
}