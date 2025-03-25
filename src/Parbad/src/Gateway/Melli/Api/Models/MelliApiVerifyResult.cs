// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Melli.Api.Models;

public class MelliApiVerifyResultModel
{
    public int? ResCode { get; set; }

    public long Amount { get; set; }

    public string Description { get; set; }

    public string RetrivalRefNo { get; set; }

    public string SystemTraceNo { get; set; }

    public long OrderId { get; set; }
}
