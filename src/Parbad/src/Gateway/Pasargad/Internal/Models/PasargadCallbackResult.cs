// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Parbad.Gateway.Pasargad.Internal.Models
{
    internal class PasargadCallbackResult
    {
        public bool IsSucceed { get; set; }

        /// <summary>
        /// Equals to ReferenceID in Parbad system.
        /// </summary>
        public string InvoiceNumber { get; set; }

        public string InvoiceDate { get; set; }

        public string TransactionId { get; set; }

        public IEnumerable<KeyValuePair<string, string>> CallbackCheckData { get; set; }

        public string Message { get; set; }
    }
}
