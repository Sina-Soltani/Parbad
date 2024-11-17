using System;
using Parbad.Storage.Abstractions;
using Parbad.Storage.EntityFrameworkCore.Context;

namespace Parbad.Storage.EntityFrameworkCore;

/// <summary>
/// EntityFramework Core implementation of <see cref="IStorageManager"/>.
/// </summary>
[Obsolete("This class will be removed in a future release. The implementations are moved to the EntityFrameworkCoreStorage class.")]
public class EntityFrameworkCoreStorageManager(ParbadDataContext context) : EntityFrameworkCoreStorage(context), IStorageManager;
