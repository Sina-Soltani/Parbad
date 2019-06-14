using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.Mvc;
using Microsoft.Owin;
using Owin;
using Parbad.Builder;
using Parbad.Sample.Mvc;
using Parbad.Sample.Mvc.Controllers;

[assembly: OwinStartup(typeof(Startup))]

namespace Parbad.Sample.Mvc
{
    // This file is an example of How to integrate Parbad with a Dependency Injection library such as Autofac.
    public class Startup
    {
        public void ConfigureServices(ContainerBuilder containerBuilder)
        {
            // Default MVC Configurations
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Register MVC controllers using Autofac
            containerBuilder.RegisterControllers(typeof(HomeController).Assembly);

            // Parbad configuration
            var parbadBuilder = ParbadConfig.Configure();

            // Register Parbad using Autofac.
            containerBuilder.Populate(parbadBuilder.Services);
        }

        public void Configuration(IAppBuilder app)
        {
            // Default Autofac MVC settings.

            var containerBuilder = new ContainerBuilder();

            ConfigureServices(containerBuilder);

            var container = containerBuilder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            app.UseAutofacMiddleware(container);
            app.UseAutofacMvc();

            // Add the Parbad Virtual Gateway (if you need)
            // Get IServiceProvider required by Parbad Virtual Gateway from Autofac.
            var serviceProvider = container.Resolve<IServiceProvider>();
            app.UseParbadVirtualGateway(serviceProvider);
        }
    }
}
