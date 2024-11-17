// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;

namespace Parbad.Storage.Abstractions;

/// <summary>
/// Provides the APIs for managing payments and transactions in a persistence store.
/// </summary>
[Obsolete("This interface will be removed in a future release. All methods are moved to IStorage interface.")]
public interface IStorageManager : IStorage;
