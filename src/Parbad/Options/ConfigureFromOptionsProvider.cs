// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Options;

namespace Parbad.Options
{
    public class ConfigureFromOptionsProvider<TOptions> : IConfigureOptions<TOptions> where TOptions : class, new()
    {
        public ConfigureFromOptionsProvider(IParbadOptionsProvider<TOptions> optionsProvider)
        {
            OptionsProvider = optionsProvider;
        }

        public IParbadOptionsProvider<TOptions> OptionsProvider { get; }

        public void Configure(TOptions options)
        {
            OptionsProvider.Provide(options);
        }
    }
}
