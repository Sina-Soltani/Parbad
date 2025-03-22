// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

namespace Parbad.Gateway.Pasargad;

/// <summary>
/// Additional Data that can be sent to Pasargad gateway when requesting a token.
/// </summary>
public class PasargadRequestAdditionalData
{
    public string Description { get; set; }

    public string PayerMail { get; set; }

    public string NationalCode { get; set; }

    public string MobileNumber { get; set; }

    public string PayerName { get; set; }

    public string Pans { get; set; }
}
