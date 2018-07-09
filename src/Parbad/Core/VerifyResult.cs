using Parbad.Providers;

namespace Parbad.Core
{
    public class VerifyResult
    {
        internal VerifyResult(Gateway gateway, string referenceId, string transactionId, VerifyResultStatus status, string message)
        {
            Gateway = gateway;
            ReferenceId = referenceId;
            TransactionId = transactionId;
            Status = status;
            Message = message;
        }

        public Gateway Gateway { get; }

        public string ReferenceId { get; }

        public string TransactionId { get; }

        public VerifyResultStatus Status { get; }

        public string Message { get; }
    }
}