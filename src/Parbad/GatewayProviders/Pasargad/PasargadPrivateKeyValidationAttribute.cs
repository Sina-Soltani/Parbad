// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace Parbad.GatewayProviders.Pasargad
{
    public class PasargadPrivateKeyValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return !(value is string privateKey) || PasargadHelper.IsPrivateKeyValid(privateKey);
        }
    }
}
