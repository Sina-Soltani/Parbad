using Microsoft.AspNetCore.Http;
using System;

namespace Parbad.Options
{
    /// <summary>
    /// Provides configuration for Parbad.
    /// </summary>
    public class ParbadOptions
    {
        /// <summary>
        /// Enables or disables the logging. The default value is true.
        /// </summary>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Contains all messages that Parbad uses in results.
        /// </summary>
        public MessagesOptions Messages { get; set; } = new MessagesOptions();

        /// <summary>
        /// A delegate that allows for the generation of a nonce (a unique token) based on the HttpContext.
        /// </summary>
        public Func<HttpContext, string> NonceFactory { get; set; }
    }
}
