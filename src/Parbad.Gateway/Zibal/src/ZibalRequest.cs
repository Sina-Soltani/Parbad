namespace Parbad.Gateway.Zibal
{
    public class ZibalRequest
    {
        public string? Description { get; set; }
        public string? CustomerMobile { get; set; }

        /// <summary>
        /// Send PaymentUrl For Customer (CustomerMobile)
        /// </summary>
        public bool SendSms { get; set; } = false;

        /// <summary>
        /// value should between 0 - 2
        /// </summary>
        /// <value>0 - Deduction from the transaction | 1 - Deduction from the wallet | 2 - Add to payment amount</value>
        public int FeeMode { get; set; }

    }
}