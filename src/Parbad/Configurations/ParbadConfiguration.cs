using Parbad.Infrastructure.Data;
using Parbad.Infrastructure.Data.Providers;

namespace Parbad.Configurations
{
    public static class ParbadConfiguration
    {
        static ParbadConfiguration()
        {
            Gateways = new GatewaysConfiguration();

            Storage = new TemporaryMemoryStorage(TemporaryMemoryStorage.DefaultInvoiceLifetime);

            Logger = new LoggerSettings();
        }

        /// <summary>
        /// Gateways configuration
        /// </summary>
        public static GatewaysConfiguration Gateways { get; }

        /// <summary>
        /// Storage for saving and loading payment's data.
        /// <para>TemporaryMemoryStorage is assigned by default.</para>
        /// <para>You can also implement your custom storage by implementing Parbad.Infrastructure.Data.Storage and then assign it to this property.</para>
        /// </summary>
        public static Storage Storage { get; set; }

        /// <summary>
        /// Logger settings
        /// </summary>
        public static LoggerSettings Logger { get; }
    }
}