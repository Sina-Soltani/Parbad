using System;

namespace Parbad.Gateway.Zibal.Internal
{
    internal class ZibalVerifyResponseModel
    {
        public DateTime? PaidAt { get; set; }
        public string CardNumber { get; set; }
        public int Status { get; set; }

        public int Amount
        {
            //Convert To 'تومان'
            get => Amount * 10;
            set => Amount = value;
        }

        public long RefNumber { get; set; }
        public string Description { get; set; }
        public long OrderId { get; set; }
        public int Result { get; set; }
        public string Message { get; set; }
    }
}