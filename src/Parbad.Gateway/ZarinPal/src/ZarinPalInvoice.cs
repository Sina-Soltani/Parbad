// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using Parbad.Internal;

namespace Parbad.Gateway.ZarinPal
{
    /// <summary>
    /// Describes an invoice for ZarinPal gateway.
    /// </summary>
    public class ZarinPalInvoice
    {
        /// <summary>
        /// Initializes an instance of <see cref="ZarinPalInvoice"/>.
        /// </summary>
        /// <param name="description">A short description about this invoice which is required by ZarinPal gateway.</param>
        public ZarinPalInvoice(string description)
            : this(description, null, null)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="ZarinPalInvoice"/>.
        /// </summary>
        /// <param name="description">A short description about this invoice which is required by ZarinPal gateway.</param>
        /// <param name="email">Buyer's email.</param>
        /// <param name="mobile">Buyer's mobile.</param>
        public ZarinPalInvoice(string description, string email, string mobile)
        {
            if (description.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(description));
            }

            Description = description;
            Email = email;
            Mobile = mobile;
        }

        /// <summary>
        /// A short description about this invoice which is required by ZarinPal gateway.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Buyer's email.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Buyer's mobile.
        /// </summary>
        public string Mobile { get; }
    }
}
