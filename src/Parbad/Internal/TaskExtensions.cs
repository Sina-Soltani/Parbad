// Copyright (c) Parbad. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC License, Version 3.0. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Parbad.Internal
{
    internal static class TaskExtensions
    {
        public static ConfiguredTaskAwaitable ConfigureAwaitFalse(this Task task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            return task.ConfigureAwait(false);
        }

        public static ConfiguredTaskAwaitable<TResult> ConfigureAwaitFalse<TResult>(this Task<TResult> task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));

            return task.ConfigureAwait(false);
        }
    }
}
