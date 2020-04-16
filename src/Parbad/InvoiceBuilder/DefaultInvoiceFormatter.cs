// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Parbad.Abstraction;

namespace Parbad.InvoiceBuilder
{
    /// <summary>
    /// Default implementation of <see cref="IInvoiceFormatter"/>.
    /// </summary>
    public class DefaultInvoiceFormatter : IInvoiceFormatter
    {
        private readonly Action<Invoice> _formatInvoice;
        private readonly Func<Invoice, Task> _formatInvoiceAsync;

        /// <summary>
        /// Initializes an instance of <see cref="DefaultInvoiceFormatter"/>.
        /// </summary>
        /// <param name="formatInvoice">Format the given invoice.</param>
        public DefaultInvoiceFormatter(Action<Invoice> formatInvoice)
        {
            _formatInvoice = formatInvoice ?? throw new ArgumentNullException(nameof(formatInvoice));
            _formatInvoiceAsync = null;
        }

        /// <summary>
        /// Initializes an instance of <see cref="DefaultInvoiceFormatter"/>.
        /// </summary>
        /// <param name="formatInvoiceAsync">Format the given invoice asynchronously.</param>
        public DefaultInvoiceFormatter(Func<Invoice, Task> formatInvoiceAsync)
        {
            _formatInvoice = null;
            _formatInvoiceAsync = formatInvoiceAsync ?? throw new ArgumentNullException(nameof(formatInvoiceAsync));
        }

        /// <inheritdoc />
        public Task FormatAsync(Invoice invoice, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_formatInvoice != null)
            {
                _formatInvoice(invoice);

                return Task.CompletedTask;
            }

            return _formatInvoiceAsync(invoice);
        }
    }
}
