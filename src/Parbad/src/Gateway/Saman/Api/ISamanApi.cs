// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Parbad.Gateway.Saman.Api.Models;

namespace Parbad.Gateway.Saman.Api;

public interface ISamanApi
{
    Task<SamanTokenResponse> RequestToken(SamanTokenRequest request,
                                          CancellationToken cancellationToken = default);

    Task<SamanVerificationAndReverseResponse> Verify(SamanVerificationRequest request,
                                                     CancellationToken cancellationToken = default);

    Task<SamanVerificationAndReverseResponse> Reverse(SamanReverseRequest request,
                                                      CancellationToken cancellationToken = default);
}
