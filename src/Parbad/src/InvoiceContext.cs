// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Storage.Abstractions.Models;
using System.Collections.Generic;

namespace Parbad
{
    /// <summary>
    /// Describes the payment and transactions of a specific invoice.
    /// </summary>
    public class InvoiceContext
    {
        /// <summary>
        /// Initializes an instance of <see cref="InvoiceContext"/>.
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="transactions"></param>
        public InvoiceContext(Payment payment, IEnumerable<Transaction> transactions)
        {
            Payment = payment;
            Transactions = transactions;
        }

        /// <summary>
        /// Contains the payment information.
        /// </summary>
        public Payment Payment { get; }

        /// <summary>
        /// Contains the transactions of this invoice.
        /// </summary>
        public IEnumerable<Transaction> Transactions { get; }
    }
}
