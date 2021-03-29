namespace Parbad.Gateway.PayPing.Internal
{
    internal class CreatePayRequestModel
    {
        public decimal Amount { get; set; }
        public string PayerIdentity { get; set; }
        public string PayerName { get; set; }
        public string Description { get; set; }
        public string ClientRefId { get; set; }
        public string ReturnUrl { get; set; }
    }
}