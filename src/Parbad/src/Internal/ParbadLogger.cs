// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parbad.Options;
using System;

namespace Parbad.Internal
{
    internal class ParbadLogger<TCategoryName> : IParbadLogger<TCategoryName>
    {
        private readonly ParbadOptions _options;
        private readonly ILogger<TCategoryName> _logger;

        public ParbadLogger(IOptions<ParbadOptions> options, ILogger<TCategoryName> logger)
        {
            _logger = logger;
            _options = options.Value;
        }

        public virtual void Log(Action<ILogger<TCategoryName>> logger)
        {
            if (!_options.EnableLogging) return;

            logger(_logger);
        }
    }
}
