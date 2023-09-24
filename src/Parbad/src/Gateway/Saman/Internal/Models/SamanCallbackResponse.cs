// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Saman.Internal.Models;

internal class SamanCallbackResponse
{
    public string? MID { get; set; }

    public string? State { get; set; }

    public string? Status { get; set; }
    
    public string? Rrn { get; set; }

    public string? RefNum { get; set; }

    public string? ResNum { get; set; }

    public string? TerminalId { get; set; }

    public string? Amount { get; set; }

    public string? Wage { get; set; }

    public string? TraceNo { get; set; }

    public string? SecurePan { get; set; }

    public string? HashedCardNumber { get; set; }
}
