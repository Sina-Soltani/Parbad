using Parbad.Infrastructure.Logging;

namespace Parbad.Configurations
{
    /// <summary>
    /// Logger settings.
    /// </summary>
    public class LoggerSettings
    {
        /// <summary>
        /// A logger that logs payment operations.
        /// </summary>
        public Logger Provider { get; set; }

        /// <summary>
        /// If you defined any handlers for Parbad.Web.LogViewer.ParbadLogViewerHandler inside web.config, then set path of handler.
        /// </summary>
        public string LogViewerHandlerPath { get; set; }
    }
}