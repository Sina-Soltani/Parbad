// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Parbad.InvoiceBuilder;

namespace Parbad.GatewayProviders.Mellat
{
    /// <summary>
    /// A builder which helps to build the extra functions of Mellat Gateway.
    /// </summary>
    public interface IMellatGatewayInvoiceBuilder
    {
        IInvoiceBuilder InvoiceBuilder { get; }

        IMellatGatewayInvoiceBuilder AddMellatCumulativeAccount(long subServiceId, long amount);

        IMellatGatewayInvoiceBuilder AddMellatCumulativeAccount(long subServiceId, long amount, long payerId);

        IMellatGatewayInvoiceBuilder AddMellatCumulativeAccounts(IList<MellatCumulativeDynamicAccount> accounts);
    }
}
