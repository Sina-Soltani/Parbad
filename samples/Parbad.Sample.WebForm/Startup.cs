using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Owin;
using Owin;
using Parbad.Builder;
using Parbad.Sample.WebForm;

[assembly: OwinStartup(typeof(Startup))]

namespace Parbad.Sample.WebForm
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Parbad configuration
            var parbad = ParbadConfig.Configure();

            // Use Virtual Gateway
            app.UseParbadVirtualGateway(parbad.Services);
        }
    }
}
