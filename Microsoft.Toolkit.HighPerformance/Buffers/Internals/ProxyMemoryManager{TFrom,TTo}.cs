﻿using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.HighPerformance.Buffers.Internals.Interfaces;

namespace Microsoft.Toolkit.HighPerformance.Buffers.Internals
{
    /// <summary>
    /// A custom <see cref="MemoryManager{T}"/> that casts data from a <see cref="MemoryManager{T}"/> of <typeparamref name="TFrom"/>, to <typeparamref name="TTo"/> values.
    /// </summary>
    /// <typeparam name="TFrom">The source type of items to read.</typeparam>
    /// <typeparam name="TTo">The target type to cast the source items to.</typeparam>
    internal sealed class ProxyMemoryManager<TFrom, TTo> : MemoryManager<TTo>, IMemoryManager
        where TFrom : unmanaged
        where TTo : unmanaged
    {
        /// <summary>
        /// The source <see cref="MemoryManager{T}"/> to read data from.
        /// </summary>
        private readonly MemoryManager<TFrom> memoryManager;

        /// <summary>
        /// The starting offset within <see name="memoryManager"/>.
        /// </summary>
        private readonly int offset;

        /// <summary>
        /// The original used length for <see name="memoryManager"/>.
        /// </summary>
        private readonly int length;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyMemoryManager{TFrom, TTo}"/> class.
        /// </summary>
        /// <param name="memoryManager">The source <see cref="MemoryManager{T}"/> to read data from.</param>
        /// <param name="offset">The starting offset within <paramref name="memoryManager"/>.</param>
        /// <param name="length">The original used length for <paramref name="memoryManager"/>.</param>
        public ProxyMemoryManager(MemoryManager<TFrom> memoryManager, int offset, int length)
        {
            this.memoryManager = memoryManager;
            this.offset = offset;
            this.length = length;
        }

        /// <inheritdoc/>
        public override Span<TTo> GetSpan()
        {
            Span<TFrom> span = this.memoryManager.GetSpan().Slice(this.offset, this.length);

            return MemoryMarshal.Cast<TFrom, TTo>(span);
        }

        /// <inheritdoc/>
        public override MemoryHandle Pin(int elementIndex = 0)
        {
            int byteOffset = elementIndex * Unsafe.SizeOf<TTo>();

#if NETSTANDARD1_4
            int
                shiftedOffset = byteOffset / Unsafe.SizeOf<TFrom>(),
                remainder = byteOffset - (shiftedOffset * Unsafe.SizeOf<TFrom>());
#else
            int shiftedOffset = Math.DivRem(byteOffset, Unsafe.SizeOf<TFrom>(), out int remainder);
#endif

            if (remainder != 0)
            {
                ThrowArgumentExceptionForInvalidAlignment();
            }

            return this.memoryManager.Pin(this.length + shiftedOffset);
        }

        /// <inheritdoc/>
        public override void Unpin()
        {
            this.memoryManager.Unpin();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            ((IDisposable)this.memoryManager).Dispose();
        }

        /// <inheritdoc/>
        public MemoryManager<T> Cast<T>(int offset, int length)
            where T : unmanaged
        {
            return new ProxyMemoryManager<TFrom, T>(this.memoryManager, this.offset + offset, length);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentOutOfRangeException"/> when <see cref="Pin"/> receives an invalid target index.
        /// </summary>
        private static void ThrowArgumentExceptionForInvalidAlignment()
        {
            throw new ArgumentOutOfRangeException("elementIndex", "The input index doesn't result in an aligned item access");
        }
    }
}
