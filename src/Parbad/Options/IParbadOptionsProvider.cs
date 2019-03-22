// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Options
{
    /// <summary>
    /// Defines a mechanism for providing and configuring an option.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IParbadOptionsProvider<in TOptions> where TOptions : class, new()
    {
        /// <summary>
        /// Configures the given <paramref name="options"/>.
        /// </summary>
        /// <param name="options"></param>
        void Provide(TOptions options);
    }
}
