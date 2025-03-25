// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Melli.Internal.Models;

internal class MelliCallbackResult
{
    public int ResCode { get; set; }

    public string Token { get; set; }

    public long OrderId { get; set; }
    
    public bool IsSucceeded { get; set; }
    
    public string Message { get; set; }
}
