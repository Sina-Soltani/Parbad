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
        private readonly ILogger<TCategoryName> _logger;
        private readonly ParbadOptions _options;

        public ParbadLogger(ILogger<TCategoryName> logger, IOptions<ParbadOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (options == null) throw new ArgumentNullException(nameof(options));
            _options = options.Value;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!_options.EnableLogging) return;

            _logger.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _options.EnableLogging;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }
    }
}
