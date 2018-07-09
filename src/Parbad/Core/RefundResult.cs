using Parbad.Providers;

namespace Parbad.Core
{
    public class RefundResult
    {
        internal RefundResult(Gateway gateway, long amount, RefundResultStatus status, string message)
        {
            Gateway = gateway;
            Amount = amount;
            Status = status;
            Message = message;
        }

        public Gateway Gateway { get; }

        public long Amount { get; }

        public RefundResultStatus Status { get; }

        public string Message { get; }
    }
}