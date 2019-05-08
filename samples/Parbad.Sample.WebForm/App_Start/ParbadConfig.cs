using Microsoft.EntityFrameworkCore;
using Parbad.Builder;

namespace Parbad.Sample.WebForm
{
    public static class ParbadConfig
    {
        public static IOnlinePaymentAccessor Configure()
        {
            return
                ParbadBuilder.CreateDefaultBuilder()
                    .ConfigureGateways(gateways =>
                    {
                        gateways
                            .AddMellat()
                            .WithOptions(options =>
                            {
                                options.TerminalId = 123;
                                options.UserName = "foo";
                                options.UserPassword = "bar";
                            });

                        gateways
                            .AddParbadVirtual()
                            .WithOptions(options => options.GatewayPath = "/virtual");
                    })
                    .ConfigureHttpContext(builder => builder.UseOwinFromCurrentHttpContext())
                    .ConfigureDatabase(builder =>
                    {
                        // In-Memory (For testing and development only)
                        //builder.UseInMemoryDatabase("MyDatabase");

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
                    })
                    .Build(); // don't forget to use the build method. Otherwise you cannot use the StaticOnlinePayment class.
        }
    }
}
