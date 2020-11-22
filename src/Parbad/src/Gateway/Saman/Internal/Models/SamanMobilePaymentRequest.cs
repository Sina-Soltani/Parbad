namespace Parbad.Gateway.Saman.Internal.Models
{
    internal class SamanMobilePaymentTokenRequest
    {
        public string Action { get; set; }

        public string TerminalId { get; set; }

        public string RedirectUrl { get; set; }

        public string ResNum { get; set; }

        public long Amount { get; set; }

        public long? CellNumber { get; set; }
    }
}
