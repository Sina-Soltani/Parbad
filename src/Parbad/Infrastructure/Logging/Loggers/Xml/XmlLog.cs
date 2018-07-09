using System;
using System.Xml.Serialization;
using Parbad.Providers;

namespace Parbad.Infrastructure.Logging.Loggers.Xml
{
    [XmlRoot("Log")]
    public class XmlLog
    {
        public LogType Type { get; set; }

        public Gateway Gateway { get; set; }

        public long OrderNumber { get; set; }

        public decimal Amount { get; set; }

        [XmlElement("ReferenceID")]
        public string ReferenceId { get; set; }

        [XmlElement("TransactionID")]
        public string TransactionId { get; set; }

        public string Status { get; set; }

        public string Message { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}