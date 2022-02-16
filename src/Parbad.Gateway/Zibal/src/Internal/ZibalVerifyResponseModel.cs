using System;
using Newtonsoft.Json;

namespace Parbad.Gateway.Zibal.Internal
{
    internal class ZibalVerifyResponseModel
    {
        public DateTime? PaidAt { get; set; }
        public string CardNumber { get; set; }
        public int? Status { get; set; }
        public int Amount { get; set; }
        public long? RefNumber { get; set; }
        public string? Description { get; set; }
        public string OrderId { get; set; }
        public int Result { get; set; }
        public string Message { get; set; }
    }
}