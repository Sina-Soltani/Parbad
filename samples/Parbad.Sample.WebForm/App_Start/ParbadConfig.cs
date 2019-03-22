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
                    // Uncomment the bellow code to use SQL Server instead of Memory.
                    //.ConfigureStorage(builder => builder.UseParbadSqlServer("Connection String"))
                    .ConfigureStorage(builder => builder.UseInMemoryDatabase("MyDatabase"))
                    .ConfigureHttpContext(builder => builder.UseOwinFromCurrentHttpContext())
                    .Build();
        }
    }
}
