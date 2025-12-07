namespace Parbad.Gateway.Tara
{
    public class TaraGatewayOptions
    {
        /// <summary>
        /// Production base URL for Tara API.
        /// </summary>
        public const string ProductionBaseUrl = "https://pay.tara360.ir/pay";

        /// <summary>
        /// Staging/Test base URL for Tara API.
        /// </summary>
        public const string StagingBaseUrl = "https://stage-pay.tara360.ir/pay";

        /// <summary>
        /// Gets or sets the base URL for Tara API.
        /// </summary>
        public string BaseUrl { get; set; } = ProductionBaseUrl;

        /// <summary>
        /// Gets or sets the authentication URL.
        /// </summary>
        public string AuthenticationUrl { get; set; } = $"{ProductionBaseUrl}/api/v2/authenticate";

        /// <summary>
        /// Gets or sets the get token URL.
        /// </summary>
        public string GetTokenUrl { get; set; } = $"{ProductionBaseUrl}/api/getToken";

        /// <summary>
        /// Gets or sets the payment request URL.
        /// </summary>
        public string PaymentUrl { get; set; } = $"{ProductionBaseUrl}/api/ipgPurchase";

        /// <summary>
        /// Gets or sets the payment verification URL.
        /// </summary>
        public string VerifyUrl { get; set; } = $"{ProductionBaseUrl}/api/purchaseVerify";

        /// <summary>
        /// Gets or sets the payment inquiry URL.
        /// </summary>
        public string InquiryUrl { get; set; } = $"{ProductionBaseUrl}/api/purchaseInquiry";

        /// <summary>
        /// Gets the appropriate URL based on test mode.
        /// Replaces production URL with staging URL if isTest is true.
        /// </summary>
        /// <param name="url">The configured URL</param>
        /// <param name="isTest">Whether to use staging environment</param>
        /// <returns>Production or staging URL</returns>
        public static string GetEnvironmentUrl(string url, bool isTest)
        {
            return isTest 
                ? url.Replace(ProductionBaseUrl, StagingBaseUrl)
                : url;
        }
    }
}

