// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Parbad.Gateway.Tara.Internal.Models;
using System.Collections.Generic;

namespace Parbad.Gateway.Tara
{
    /// <summary>
    /// Additional data for Tara gateway payment request.
    /// </summary>
    public class TaraRequestAdditionalData
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TaraRequestAdditionalData"/>.
        /// </summary>
        public TaraRequestAdditionalData()
        {
            ServiceAmountList = new List<TaraServiceAmount>();
            InvoiceItemList = new List<TaraInvoiceItem>();
        }

        /// <summary>
        /// List of service amounts for the payment request.
        /// </summary>
        public List<TaraServiceAmount> ServiceAmountList { get; set; }

        /// <summary>
        /// List of invoice items for the payment request.
        /// </summary>
        public List<TaraInvoiceItem> InvoiceItemList { get; set; }

        /// <summary>
        /// Additional data string.
        /// </summary>
        public string AdditionalData { get; set; }

        /// <summary>
        /// Mobile number of the customer.
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// VAT (Value Added Tax) amount.
        /// </summary>
        public long Vat { get; set; }
    }
}




