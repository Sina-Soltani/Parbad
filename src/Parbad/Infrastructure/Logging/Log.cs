using System;
using Parbad.Providers;

namespace Parbad.Infrastructure.Logging
{
    [Serializable]
    public class Log
    {
        public LogType Type { get; set; }

        public Gateway Gateway { get; set; }

        public long OrderNumber { get; set; }

        public long Amount { get; set; }

        public string ReferenceId { get; set; }

        public string TransactionId { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public DateTime CreatedOn { get; set; }

        public override string ToString()
        {
            return $"Type: {Type}, Gateway: {Gateway}, OrderNumber: {OrderNumber}, Amount: {Amount}, ReferenceId: {ReferenceId}, TransactionId: {TransactionId}, Status: {Status}, Message: {Message}, CreatedOn: {CreatedOn}";
        }
    }
}