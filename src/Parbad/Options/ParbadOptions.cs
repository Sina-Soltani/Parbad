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
    }
}
