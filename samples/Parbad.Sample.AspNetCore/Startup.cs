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
                        .WithOptions(options => options.GatewayPath = "/MyVirtualGateway");
                })
                .ConfigureHttpContext(builder => builder.UseDefaultAspNetCore())
                .ConfigureDatabase(builder =>
                {
                    // In-Memory (For testing and development only)
                    builder.UseInMemoryDatabase("MyDatabase");

                    // SQL Server
                    //builder.UseSqlServer("Connection String", options => options.UseParbadMigrations());

                    // MySQL
                    //builder.UseMySQL("Connection String", options => options.UseParbadMigrations());

                    // Sqlite
                    //builder.UseSqlite("Connection String");
                })
                .ConfigureDatabaseInitializers(builder =>
                {
                    // For In-Memory
                    builder.CreateDatabase();

                    // (SQL Server, MySQL, etc.)
                    //builder.CreateAndMigrateDatabase();

                    // (Sqlite, etc.)
                    //builder.DeleteAndCreateDatabase();

                    // Define a custom database initializer
                    //builder.UseInitializer(async context =>
                    //{
                    //    await context.Database.EnsureDeletedAsync();
                    //    await context.Database.EnsureCreatedAsync();
                    //    await context.Database.MigrateAsync();
                    //});
                });
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
