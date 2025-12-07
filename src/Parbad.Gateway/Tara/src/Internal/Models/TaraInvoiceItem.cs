namespace Parbad.Gateway.Tara.Internal.Models
{
    /// <summary>
    /// Represents an invoice item in Tara gateway request.
    /// </summary>
    public class TaraInvoiceItem
    {
        /// <summary>
        /// Item name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Item code
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// Item count/quantity
        /// </summary>
        public long count { get; set; }

        /// <summary>
        /// Unit type
        /// </summary>
        public long unit { get; set; }

        /// <summary>
        /// Item fee/price
        /// </summary>
        public long fee { get; set; }

        /// <summary>
        /// Item group
        /// </summary>
        public string group { get; set; }

        /// <summary>
        /// Item group title
        /// </summary>
        public string groupTitle { get; set; }

        /// <summary>
        /// Additional data for this item
        /// </summary>
        public string data { get; set; }
    }
}

