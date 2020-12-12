using Parbad.Builder;
using Parbad.Gateway.Mellat;
using Parbad.Gateway.ParbadVirtual;

namespace Parbad.Sample.WebForm
{
    public static class ParbadConfig
    {
        public static IOnlinePaymentAccessor Configure()
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
                .ConfigureStorage(builder => builder.UseMemoryCache())
                .Build(); // don't forget to call the build method. Otherwise you cannot use the StaticOnlinePayment class.
        }
    }
}
