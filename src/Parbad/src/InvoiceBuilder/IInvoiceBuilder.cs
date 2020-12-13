// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Parbad.Abstraction;

namespace Parbad.InvoiceBuilder
{
    /// <summary>
    /// A builder which helps to build an invoice.
    /// </summary>
    public interface IInvoiceBuilder
    {
        /// <summary>
        /// Adds the given <paramref name="formatter"/> to the list of formatters.
        /// </summary>
        /// <param name="formatter"></param>
        IInvoiceBuilder AddFormatter(IInvoiceFormatter formatter);

        /// <summary>
        /// Adds the given formatter to the list of formatters.
        /// </summary>
        /// <typeparam name="T">Type of the formatter.</typeparam>
        IInvoiceBuilder AddFormatter<T>() where T : class, IInvoiceFormatter;

        /// <summary>
        /// Builds an invoice with the given data.
        /// </summary>
        Task<Invoice> BuildAsync(CancellationToken cancellationToken = default);
    }
}
