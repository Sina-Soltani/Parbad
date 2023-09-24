// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Saman.Internal.Models;

internal class SamanVerificationAndRefundResponse
{
    public TransactionDetail TransactionDetail { get; set; }

    public int ResultCode { get; set; }

    public string ResultDescription { get; set; }

    public bool Success { get; set; }
}

internal class TransactionDetail
{
    public string Rrn { get; set; }
    
    public string RefNum { get; set; }
    
    public string MaskedPan { get; set; }
    
    public string HashedPan { get; set; }
    
    public long TerminalNumber { get; set; }
    
    public long OrginalAmount { get; set; }
    
    public long AffectiveAmount { get; set; }
    
    public string StraceDate { get; set; }
    
    public string StraceNo { get; set; }
}
