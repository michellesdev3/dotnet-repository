// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Microsoft.Toolkit.Extensions
{
    /// <summary>
    /// Helpers for working with tasks.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Gets the result of a <see cref="Task{TResult}"/> if available, or <see langword="default"/> otherwise.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Task{TResult}"/> to get the result for.</typeparam>
        /// <param name="task">The input <see cref="Task{TResult}"/> instance to get the result for.</param>
        /// <returns>The result of <paramref name="task"/> if completed successfully, or <see langword="default"/> otherwise.</returns>
        /// <remarks>This method does not block if <paramref name="task"/> has not completed yet.</remarks>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ResultOrDefault<T>(this Task<T> task)
        {
            return task.Status == TaskStatus.RanToCompletion ? task.Result : default;
        }
    }
}
