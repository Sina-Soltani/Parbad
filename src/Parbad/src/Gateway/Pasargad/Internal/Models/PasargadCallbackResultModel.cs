// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Pasargad.Internal.Models;

internal class PasargadCallbackResultModel
{
    public bool IsSucceed => !string.IsNullOrWhiteSpace(InvoiceNumber) &&
                             !string.IsNullOrWhiteSpace(InvoiceDate) &&
                             !string.IsNullOrWhiteSpace(TransactionReferenceId);

    public string InvoiceNumber { get; set; }

    public string InvoiceDate { get; set; }

    public string TransactionReferenceId { get; set; }
}
