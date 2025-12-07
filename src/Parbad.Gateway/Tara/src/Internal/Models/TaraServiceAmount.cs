namespace Parbad.Gateway.Tara.Internal.Models
{
    /// <summary>
    /// Represents a service amount in Tara gateway request.
    /// </summary>
    public class TaraServiceAmount
    {
        /// <summary>
        /// Service identifier
        /// </summary>
        public long serviceId { get; set; }

        /// <summary>
        /// Amount for this service
        /// </summary>
        public long amount { get; set; }
    }
}

