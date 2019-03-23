using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Builder;

namespace Parbad.Sample.AspNetCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddParbad()
                .ConfigureGateways(gateways =>
                {
                    gateways
                        .AddMellat()
                        .WithOptions(options =>
                        {
                            options.TerminalId = 123;
                            options.UserName = "MyId";
                            options.UserPassword = "MyPassword";
                        });

                    gateways
                        .AddParbadVirtual()
                        .WithOptions(options => { options.GatewayPath = "/MyVirtualGateway"; });
                })
                .ConfigureHttpContext(builder => builder.UseDefaultAspNetCore())
                // Uncomment the bellow code to use SQL Server instead of Memory.
                //.ConfigureStorage(builder => builder.UseParbadSqlServer("Connection String"))
                .ConfigureStorage(builder => builder.UseInMemoryDatabase("MyDatabase"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseParbadVirtualGatewayIfDevelopment();
        }
    }
}
