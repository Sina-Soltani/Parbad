using System.Web.Hosting;
using Parbad.Configurations;
using Parbad.Infrastructure.Logging.Loggers.Xml;
using Parbad.Providers.Mellat;
using Parbad.Providers.Melli;
using Parbad.Providers.Parbad;

namespace Parbad.Sample.Mvc
{
    public static class ParbadConfig
    {
        public static void Configure()
        {
            //  Configure the Mellat gateway
            var mellatConfig = new MellatGatewayConfiguration(1111, "my user name", "my password");
            ParbadConfiguration.Gateways.ConfigureMellat(mellatConfig);

            var melliConfig = new MelliGatewayConfiguration("My Terminal ID", "My Merchant ID", "My Merchant Key");
            ParbadConfiguration.Gateways.ConfigureMelli(melliConfig);

            //  Configure the Parbad Virtual gateway
            ParbadConfiguration.Gateways.ConfigureParbadVirtualGateway(new ParbadVirtualGatewayConfiguration("Parbad.axd"));

            // Configure Logger
            var logFilePath = HostingEnvironment.MapPath("~/App_Data/PaymentLogs/");
            ParbadConfiguration.Logger.LogViewerHandlerPath = "ParbadLog.axd";
            ParbadConfiguration.Logger.Provider = new XmlFileLogger(logFilePath);

            //  Temporary Memory Storage (the default Storage. no need to set)
            //ParbadConfiguration.Storage = new TemporaryMemoryStorage(TemporaryMemoryStorage.DefaultInvoiceLifetime);

            //  SQL Server Storage
            //ParbadConfiguration.Storage = new SqlServerStorage("Connection String", "TbPayments");

            // Custom Storage
            //ParbadConfiguration.Storage = new MyStorage();
        }
    }
}