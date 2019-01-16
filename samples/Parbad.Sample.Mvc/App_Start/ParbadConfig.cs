using System.Web.Hosting;
using Parbad.Configurations;
using Parbad.Infrastructure.Logging.Loggers.Xml;
using Parbad.Providers.Mellat;
using Parbad.Providers.Parbad;

namespace Parbad.Sample.Mvc
{
    public static class ParbadConfig
    {
        public static void Configure()
        {
            ConfigGateways();

            ConfigStorage();

            ConfigLogger();
        }

        private static void ConfigGateways()
        {
            // Mellat Gateway
            var mellatConfig = new MellatGatewayConfiguration(1111, "my user name", "my password");
            ParbadConfiguration.Gateways.ConfigureMellat(mellatConfig);

            // Parbad Virtual Gateway (Note: Use it just for development)
            ParbadConfiguration.Gateways.ConfigureParbadVirtualGateway(new ParbadVirtualGatewayConfiguration("Parbad.axd"));
        }

        private static void ConfigStorage()
        {
            // Temporary Memory Storage (the default Storage. no need to set)
            //ParbadConfiguration.Storage = new TemporaryMemoryStorage(TemporaryMemoryStorage.DefaultInvoiceLifetime);

            // SQL Server Storage
            //ParbadConfiguration.Storage = new SqlServerStorage("Connection String", "TbPayments");

            // Custom Storage
            //ParbadConfiguration.Storage = new MyStorage();
        }

        private static void ConfigLogger()
        {
            var logFilePath = HostingEnvironment.MapPath("~/App_Data/PaymentLogs/");

            ParbadConfiguration.Logger.LogViewerHandlerPath = "ParbadLog.axd";

            ParbadConfiguration.Logger.Provider = new XmlFileLogger(logFilePath);
        }
    }
}
