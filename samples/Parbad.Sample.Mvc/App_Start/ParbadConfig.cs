using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Builder;
using Parbad.Sample.Mvc.Services;

namespace Parbad.Sample.Mvc
{
    public static class ParbadConfig
    {
        public static IParbadBuilder Configure()
        {
            return
                ParbadBuilder.CreateDefaultBuilder()
                    .ConfigureGateways(gateways =>
                    {
                        gateways
                            .AddMellat()
                            .WithOptionsProvider<MellatOptionsProvider>(ServiceLifetime.Transient);

                        gateways
                            .AddParbadVirtual()
                            .WithOptions(options => options.GatewayPath = "/virtual");
                    })
                    .ConfigureHttpContext(builder => builder.UseOwinFromCurrentHttpContext())
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
                    .ConfigureDatabaseInitializer(builder =>
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
    }
}
