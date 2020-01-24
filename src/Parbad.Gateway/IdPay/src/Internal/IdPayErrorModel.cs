// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using Newtonsoft.Json;

namespace Parbad.Gateway.IdPay.Internal
{
    internal class IdPayErrorModel
    {
        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            return $"Error Code: {ErrorCode}, Error Message: {ErrorMessage}";
        }
    }
}