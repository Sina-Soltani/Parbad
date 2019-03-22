// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Parbad.Internal;
using Parbad.Options;

namespace Parbad.Builder
{
    public static class OptionsExtensions
    {
        /// <summary>
        /// Configures the options required by Parbad to an <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="optionsBuilder">A builder for configuring the options.</param>
        /// <returns></returns>
        public static IParbadBuilder ConfigureOptions(this IParbadBuilder builder,
            Action<IParbadOptionsBuilder> optionsBuilder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));

            builder.Services.AddOptions();

            optionsBuilder(new ParbadOptionsBuilder(builder.Services));

            return builder;
        }

        public static IParbadOptionsBuilder AddDataAnnotationsValidator<TOptions>(this IParbadOptionsBuilder builder)
            where TOptions : class
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddSingleton<IValidateOptions<TOptions>>(
                new DataAnnotationValidateOptions<TOptions>(Microsoft.Extensions.Options.Options.DefaultName));

            return builder;
        }
    }
}
