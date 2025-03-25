// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Parbad.Gateway.Melli.Api.Models;

namespace Parbad.Gateway.Melli.Api;

public interface IMelliApi
{
    Task<MelliApiRequestResultModel> Request(MelliApiRequestModel model, CancellationToken cancellationToken = default);

    Task<MelliApiVerifyResultModel> Verify(MelliApiVerifyModel model, CancellationToken cancellationToken = default);
}
