// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Parbad.Abstraction;
using Parbad.InvoiceBuilder;

namespace Parbad.Internal
{
    /// <inheritdoc />
    public class DefaultInvoiceBuilder : IInvoiceBuilder
    {
        private readonly IServiceProvider _services;
        private readonly List<IInvoiceFormatter> _formatters;
        private readonly List<Type> _formatterTypes;

        /// <summary>
        /// Initializes an instance of <see cref="DefaultInvoiceBuilder"/>.
        /// </summary>
        public DefaultInvoiceBuilder(IServiceProvider services)
        {
            _services = services;
            _formatters = new List<IInvoiceFormatter>();
            _formatterTypes = new List<Type>();
        }

        /// <inheritdoc />
        public IInvoiceBuilder AddFormatter(IInvoiceFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            _formatters.Add(formatter);

            return this;
        }

        /// <inheritdoc />
        public IInvoiceBuilder AddFormatter<T>() where T : class, IInvoiceFormatter
        {
            _formatterTypes.Add(typeof(T));

            return this;
        }

        /// <inheritdoc />
        public virtual async Task<Invoice> BuildAsync(CancellationToken cancellationToken = default)
        {
            var invoice = new Invoice();

            var formatters = _formatters.ToList();
            formatters.AddRange(_formatterTypes.Select(type => (IInvoiceFormatter)_services.GetRequiredService(type)));

            foreach (var formatter in formatters)
            {
                await formatter.FormatAsync(invoice);
            }

            return invoice;
        }
    }
}
