// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.FanAva.Internal.Models
{
    internal class FanAvaCallbackResultModel
    {
        public bool IsSucceed { get; set; }
        
        public string InvoiceNumber { get; set; }
        
        public string Message { get; set; }
        
        public FanAvaCheckRequestModel CallbackCheckData { get; set; }
    }
}
