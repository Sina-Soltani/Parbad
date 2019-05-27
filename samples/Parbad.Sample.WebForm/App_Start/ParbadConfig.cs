using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Builder;
using Parbad.Sample.WebForm.Services;

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
                            .WithAccounts(accounts =>
                            {
                                accounts.AddInMemory(account =>
                                {
                                    account.TerminalId = 123;
                                    account.UserName = "MyId";
                                    account.UserPassword = "MyPassword";
                                });

                                // Sample: Add the accounts from your database or a service.
                                //accounts.Add<MellatAccountSource>(ServiceLifetime.Transient);
                            });

                        gateways
                            .AddParbadVirtual();
                        //.WithOptions(options => options.GatewayPath = "/MyVirtualGateway");
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
                    })
                    .Build(); // don't forget to use the build method. Otherwise you cannot use the StaticOnlinePayment class.
        }
    }
}
