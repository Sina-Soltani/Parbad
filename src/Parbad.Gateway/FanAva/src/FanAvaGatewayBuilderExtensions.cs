using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Gateway.Pasargad;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.FanAva
{
    public static class FanAvaGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds FanAva gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<FanAvaGateway> AddFanAva(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder
                .AddGateway<FanAvaGateway>()
                .WithHttpClient(clientBuilder => {  })
                .WithOptions(options => { });
        }

        /// <summary>
        /// Configures the accounts for <see cref="FanAvaGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        /// 
        public static IGatewayConfigurationBuilder<FanAvaGateway> WithAccounts(this IGatewayConfigurationBuilder<FanAvaGateway> builder, Action<IGatewayAccountBuilder<FanAvaGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));


            return builder.WithAccounts(configureAccounts);
        }

        /// <summary>
        /// Configures the options for FanAva Gateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions">Configuration</param>
        public static IGatewayConfigurationBuilder<FanAvaGateway> WithOptions(
            this IGatewayConfigurationBuilder<FanAvaGateway> builder,
            Action<FanAvaGatewayOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }
    }
}