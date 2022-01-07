using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.GatewayBuilders;

namespace Parbad.Gateway.AsanPardakht
{
    public static class AsanPardakhtGatewayBuilderExtensions
    {
        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> AddAsanPardakht(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder
                .AddGateway<AsanPardakhtGateway>()
                .WithOptions(options => { })
                .WithHttpClient(clientBuilder => clientBuilder.ConfigureHttpClient(client => { }));
        }

        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> WithAccounts(
            this IGatewayConfigurationBuilder<AsanPardakhtGateway> builder,
            Action<IGatewayAccountBuilder<AsanPardakhtGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }

        public static IGatewayConfigurationBuilder<AsanPardakhtGateway> WithOptions(
            this IGatewayConfigurationBuilder<AsanPardakhtGateway> builder,
            Action<AsanPardakhtGatewayOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}