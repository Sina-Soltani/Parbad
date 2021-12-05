// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Gateway.AsanPardakht.Internal.Models
{
    internal class AsanPardakhtGetTransModel
    {
        public string CardNumber { get; set; }
        public string Rrn { get; set; }
        public string RefID { get; set; }
        public string Amount { get; set; }
        public long PayGateTranID { get; set; }
        public string SalesOrderID { get; set; }
        public string Hash { get; set; }
        public int ServiceTypeId { get; set; }
        public string ServiceStatusCode { get; set; }
        public string DestinationMobile { get; set; }
        public int ProductId { get; set; }
        public string ProductNameFa { get; set; }
        public int ProductPrice { get; set; }
        public int OperatorId { get; set; }
        public string OperatorNameFa { get; set; }
        public int SimTypeId { get; set; }
        public string SimTypeTitleFa { get; set; }
        public string BillId { get; set; }
        public string PayId { get; set; }
        public string BillOrganizationNameFa { get; set; }
        public DateTime PayGateTranDate { get; set; }
        public int PayGateTranDateEpoch { get; set; }
    }
}
