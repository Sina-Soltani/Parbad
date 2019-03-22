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
                    // Uncomment the bellow code to use SQL Server instead of Memory.
                    //.ConfigureStorage(builder => builder.UseParbadSqlServer("Connection String"))
                    .ConfigureStorage(builder => builder.UseInMemoryDatabase("MyDatabase"))
                    .ConfigureHttpContext(builder => builder.UseOwinFromCurrentHttpContext());
        }
    }
}
