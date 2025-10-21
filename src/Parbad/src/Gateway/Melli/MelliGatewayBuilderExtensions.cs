// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Gateway.Melli.Internal;
using Parbad.GatewayBuilders;
using Parbad.InvoiceBuilder;

namespace Parbad.Gateway.Melli
{
    public static class MelliGatewayBuilderExtensions
    {
        /// <summary>
        /// Adds Melli gateway to Parbad services.
        /// </summary>
        /// <param name="builder"></param>
        public static IGatewayConfigurationBuilder<MelliGateway> AddMelli(this IGatewayBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IMelliGatewayCrypto, MelliGatewayCrypto>();

            return builder
                .AddGateway<MelliGateway>()
                .WithHttpClient(clientBuilder => { })
                .WithOptions(options => { });
        }

        /// <summary>
        /// Configures the accounts for <see cref="MelliGateway"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureAccounts">Configures the accounts.</param>
        public static IGatewayConfigurationBuilder<MelliGateway> WithAccounts(
            this IGatewayConfigurationBuilder<MelliGateway> builder,
            Action<IGatewayAccountBuilder<MelliGatewayAccount>> configureAccounts)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            return builder.WithAccounts(configureAccounts);
        }

        /// <summary>
        /// Configures the options for AsanPardakhtGateway.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configureOptions">Configuration</param>
        public static IGatewayConfigurationBuilder<MelliGateway> WithOptions(
            this IGatewayConfigurationBuilder<MelliGateway> builder,
            Action<MelliGatewayOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);

            return builder;
        }
        
        /// <summary>
        /// اضافه کردن اطلاعات تسهیم بانک سداد ملی به درخواست
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInvoiceBuilder AddMelliCumulativeAccounts(this IInvoiceBuilder builder, MelliCumulativeAccount data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.MultiplexingRows==null || data.MultiplexingRows.Count==0)  throw new ArgumentNullException(nameof(data.MultiplexingRows));
            
            builder.ChangeProperties(properties =>
            {
                properties[MelliHelper.CumulativeAccountsKey] = data;
            });

            return builder;
        }
        
    }
}
