// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MemoryStream = Microsoft.Toolkit.HighPerformance.Streams.MemoryStream;

namespace Microsoft.Toolkit.HighPerformance.Extensions
{
    /// <summary>
    /// Helpers for working with the <see cref="Memory{T}"/> type.
    /// </summary>
    public static class MemoryExtensions
    {
        /// <summary>
        /// Casts a <see cref="Memory{T}"/> of one primitive type <typeparamref name="TFrom"/> to another primitive type <typeparamref name="TTo"/>.
        /// </summary>
        /// <typeparam name="TFrom">The type of items in the source <see cref="Memory{T}"/>.</typeparam>
        /// <typeparam name="TTo">The type of items in the destination <see cref="Memory{T}"/>.</typeparam>
        /// <param name="memory">The source <see cref="Memory{T}"/>, of type <typeparamref name="TFrom"/>.</param>
        /// <returns>A <see cref="Memory{T}"/> of type <typeparamref name="TTo"/></returns>
        /// <exception cref="ArgumentException">Thrown when the data store of <paramref name="memory"/> is not supported (eg. when it is a <see cref="string"/>).</exception>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<TTo> Cast<TFrom, TTo>(this Memory<TFrom> memory)
            where TFrom : unmanaged
            where TTo : unmanaged
        {
            return MemoryMarshal.AsMemory(((ReadOnlyMemory<TFrom>)memory).Cast<TFrom, TTo>());
        }

        /// <summary>
        /// Returns a <see cref="Stream"/> wrapping the contents of the given <see cref="Memory{T}"/> of <see cref="byte"/> instance.
        /// </summary>
        /// <param name="memory">The input <see cref="Memory{T}"/> of <see cref="byte"/> instance.</param>
        /// <returns>A <see cref="Stream"/> wrapping the data within <paramref name="memory"/>.</returns>
        /// <remarks>
        /// Since this method only receives a <see cref="Memory{T}"/> instance, which does not track
        /// the lifetime of its underlying buffer, it is responsibility of the caller to manage that.
        /// In particular, the caller must ensure that the target buffer is not disposed as long
        /// as the returned <see cref="Stream"/> is in use, to avoid unexpected issues.
        /// </remarks>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stream AsStream(this Memory<byte> memory)
        {
            return new MemoryStream(memory);
        }
    }
}
