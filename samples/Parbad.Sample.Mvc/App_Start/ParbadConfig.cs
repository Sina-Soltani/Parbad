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
            return ParbadBuilder.CreateDefaultBuilder()
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
                        });

                    gateways
                        .AddParbadVirtual()
                        .WithOptions(options => options.GatewayPath = "/MyVirtualGateway");
                })
                .ConfigureHttpContext(builder => builder.UseOwinFromCurrentHttpContext())
                .ConfigureStorage(builder => builder.UseMemoryCache());
        }
    }
}
