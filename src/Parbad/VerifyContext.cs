// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Parbad.Storage.Abstractions;

namespace Parbad
{
    public class VerifyContext
    {
        public VerifyContext(Payment payment, IEnumerable<Transaction> transactions)
        {
            Payment = payment;
            Transactions = transactions;
        }

        public Payment Payment { get; }

        public IEnumerable<Transaction> Transactions { get; }
    }
}
