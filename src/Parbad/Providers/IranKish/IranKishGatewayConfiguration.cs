namespace Parbad.Providers.IranKish
{
    /// <summary>
    /// IranKish gateway's configuration
    /// </summary>
    public class IranKishGatewayConfiguration
    {
        /// <summary>
        /// Initializes a new instance of IranKishGatewayConfiguration.
        /// </summary>
        /// <param name="merchantId">Merchant ID</param>
        public IranKishGatewayConfiguration(string merchantId)
        {
            MerchantId = merchantId;
        }

        /// <summary>
        /// Merchant ID
        /// </summary>
        public string MerchantId { get; }
    }
}