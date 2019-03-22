// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Parbad.Abstraction;

namespace Parbad.PaymentTokenProviders
{
    /// <summary>
    /// A provider which generates a token for each new payment requests.
    /// The generated token will be saved in database and retrieved when the client
    /// comes back from the gateway.
    /// </summary>
    public interface IPaymentTokenProvider
    {
        /// <summary>
        /// Generates a unique token for the given invoice.
        /// </summary>
        /// <param name="invoice">The invoice which must be paid by the client.</param>
        /// <param name="cancellationToken"></param>
        Task<string> ProvideTokenAsync(Invoice invoice, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the token.
        /// </summary>
        Task<string> RetrieveTokenAsync(CancellationToken cancellationToken = default);
    }
}
