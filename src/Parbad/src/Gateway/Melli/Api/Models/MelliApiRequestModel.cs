// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Melli.Api.Models;

public class MelliApiRequestModel
{
    public string TerminalId { get; set; }
    
    public string MerchantId { get; set; }
    
    public long Amount { get; set; }
    
    public string SignData { get; set; }
    
    public string ReturnUrl { get; set; }
    
    public string LocalDateTime { get; set; }
    
    public long OrderId { get; set; }
}
