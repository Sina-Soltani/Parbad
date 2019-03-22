// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Parbad.Data.Domain.Transactions;

namespace Parbad.Internal
{
    internal static class AdditionalDataConverter
    {
        public static IDictionary<string, string> ToDictionary(Transaction transaction)
        {
            return JsonConvert.DeserializeObject<IDictionary<string, string>>(transaction.AdditionalData);
        }

        public static string ToJson(PaymentResult paymentResult)
        {
            if (paymentResult == null) throw new ArgumentNullException(nameof(paymentResult));

            return JsonConvert.SerializeObject(paymentResult.DatabaseAdditionalData);
        }
    }
}
