// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Parbad.Gateway.FanAva
{
    public class FanAvaGatewayAdditionalDataRequest
    {
        public static string InvoicePropertyKey => "FanAvaRequestAdditionalData";

        public FanAvaGatewayAdditionalDataRequestType Type { get; set; } = FanAvaGatewayAdditionalDataRequestType.Goods;

        public string MobileNumber { get; set; }

        public string Email { get; set; }

        public string GoodsReferenceId { get; set; }

        public string MerchantGoodReferenceId { get; set; }

        public IEnumerable<FanAvaGatewayApportionmentAccount> ApportionmentAccountList { get; set; }
    }

    public enum FanAvaGatewayAdditionalDataRequestType
    {
        Goods,
        Bill
    }

    public class FanAvaGatewayApportionmentAccount
    {
        public FanAvaGatewayApportionmentAccount(string accountIban, long amount, FanAvaGatewayApportionmentAccountType type)
        {
            AccountIban = accountIban ?? throw new ArgumentNullException(nameof(accountIban));
            Amount = amount;
            Type = type;
        }

        public string AccountIban { get; }

        public long Amount { get; }

        public FanAvaGatewayApportionmentAccountType Type { get; }
    }

    public enum FanAvaGatewayApportionmentAccountType
    {
        enMain,
        enOther
    }
}
