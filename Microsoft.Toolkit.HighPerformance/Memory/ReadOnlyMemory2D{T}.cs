﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.HighPerformance.Extensions;
using Microsoft.Toolkit.HighPerformance.Memory.Internals;

namespace Microsoft.Toolkit.HighPerformance.Memory
{
    /// <summary>
    /// A readonly version of <see cref="Memory2D{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of items in the current <see cref="Memory2D{T}"/> instance.</typeparam>
    public readonly struct ReadOnlyMemory2D<T> : IEquatable<ReadOnlyMemory2D<T>>
    {
        /// <summary>
        /// The target <see cref="object"/> instance, if present.
        /// </summary>
        private readonly object? instance;

        /// <summary>
        /// The initial offset within <see cref="instance"/>.
        /// </summary>
        private readonly IntPtr offset;

        /// <summary>
        /// The height of the specified 2D region.
        /// </summary>
        private readonly int height;

        /// <summary>
        /// The width of the specified 2D region.
        /// </summary>
        private readonly int width;

        /// <summary>
        /// The pitch of the specified 2D region.
        /// </summary>
        private readonly int pitch;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyMemory2D{T}"/> struct.
        /// </summary>
        /// <param name="array">The target array to wrap.</param>
        /// <param name="offset">The initial offset within <paramref name="array"/>.</param>
        /// <param name="width">The width of each row in the resulting 2D area.</param>
        /// <param name="height">The height of the resulting 2D area.</param>
        /// <exception cref="ArrayTypeMismatchException">
        /// Thrown when <paramref name="array"/> doesn't match <typeparamref name="T"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="offset"/>, <paramref name="height"/> or <paramref name="width"/> are invalid.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory2D(T[] array, int offset, int width, int height)
            : this(array, offset, width, height, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyMemory2D{T}"/> struct.
        /// </summary>
        /// <param name="array">The target array to wrap.</param>
        /// <param name="offset">The initial offset within <paramref name="array"/>.</param>
        /// <param name="width">The width of each row in the resulting 2D area.</param>
        /// <param name="height">The height of the resulting 2D area.</param>
        /// <param name="pitch">The pitch in the resulting 2D area.</param>
        /// <exception cref="ArrayTypeMismatchException">
        /// Thrown when <paramref name="array"/> doesn't match <typeparamref name="T"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="offset"/>, <paramref name="height"/>,
        /// <paramref name="width"/> or <paramref name="pitch"/> are invalid.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory2D(T[] array, int offset, int width, int height, int pitch)
        {
            if (array.IsCovariant())
            {
                ThrowHelper.ThrowArrayTypeMismatchException();
            }

            if ((uint)offset >= (uint)array.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForOffset();
            }

            int remaining = array.Length - offset;

            if ((((uint)width + (uint)pitch) * (uint)height) > (uint)remaining)
            {
                ThrowHelper.ThrowArgumentException();
            }

            this.instance = array;
            this.offset = array.DangerousGetObjectDataByteOffset(ref array.DangerousGetReferenceAt(offset));
            this.height = height;
            this.width = width;
            this.pitch = pitch;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyMemory2D{T}"/> struct.
        /// </summary>
        /// <param name="array">The target array to wrap.</param>
        /// <exception cref="ArrayTypeMismatchException">
        /// Thrown when <paramref name="array"/> doesn't match <typeparamref name="T"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory2D(T[,]? array)
        {
            if (array is null)
            {
                this = default;

                return;
            }

            if (array.IsCovariant())
            {
                ThrowHelper.ThrowArrayTypeMismatchException();
            }

            this.instance = array;
            this.offset = array.DangerousGetObjectDataByteOffset(ref array.DangerousGetReference());
            this.height = array.GetLength(0);
            this.width = array.GetLength(1);
            this.pitch = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyMemory2D{T}"/> struct wrapping a 2D array.
        /// </summary>
        /// <param name="array">The given 2D array to wrap.</param>
        /// <param name="row">The target row to map within <paramref name="array"/>.</param>
        /// <param name="column">The target column to map within <paramref name="array"/>.</param>
        /// <param name="width">The width to map within <paramref name="array"/>.</param>
        /// <param name="height">The height to map within <paramref name="array"/>.</param>
        /// <exception cref="ArrayTypeMismatchException">
        /// Thrown when <paramref name="array"/> doesn't match <typeparamref name="T"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="height"/>, <paramref name="width"/> or <paramref name="height"/>
        /// are negative or not within the bounds that are valid for <paramref name="array"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory2D(T[,]? array, int row, int column, int width, int height)
        {
            if (array is null)
            {
                if ((row | column | width | height) != 0)
                {
                    ThrowHelper.ThrowArgumentException();
                }

                this = default;

                return;
            }

            if (array.IsCovariant())
            {
                ThrowHelper.ThrowArrayTypeMismatchException();
            }

            int
                rows = array.GetLength(0),
                columns = array.GetLength(1);

            if ((uint)row >= (uint)rows)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForRow();
            }

            if ((uint)column >= (uint)columns)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForColumn();
            }

            if (width > (columns - column))
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForWidth();
            }

            if (height > (rows - row))
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForHeight();
            }

            this.instance = array;
            this.offset = array.DangerousGetObjectDataByteOffset(ref array.DangerousGetReferenceAt(row, column));
            this.height = height;
            this.width = width;
            this.pitch = row + (array.GetLength(1) - column);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyMemory2D{T}"/> struct wrapping a layer in a 3D array.
        /// </summary>
        /// <param name="array">The given 3D array to wrap.</param>
        /// <param name="depth">The target layer to map within <paramref name="array"/>.</param>
        /// <exception cref="ArrayTypeMismatchException">
        /// Thrown when <paramref name="array"/> doesn't match <typeparamref name="T"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when either <paramref name="depth"/> is invalid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory2D(T[,,] array, int depth)
        {
            if (array.IsCovariant())
            {
                ThrowHelper.ThrowArrayTypeMismatchException();
            }

            if ((uint)depth >= (uint)array.GetLength(0))
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForDepth();
            }

            this.instance = array;
            this.offset = array.DangerousGetObjectDataByteOffset(ref array.DangerousGetReferenceAt(depth, 0, 0));
            this.height = array.GetLength(1);
            this.width = array.GetLength(2);
            this.pitch = 0;
        }

#if SPAN_RUNTIME_SUPPORT
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyMemory2D{T}"/> struct.
        /// </summary>
        /// <param name="memory">The target <see cref="Memory{T}"/> to wrap.</param>
        /// <param name="offset">The initial offset within <paramref name="memory"/>.</param>
        /// <param name="width">The width of each row in the resulting 2D area.</param>
        /// <param name="height">The height of the resulting 2D area.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="offset"/>, <paramref name="height"/> or <paramref name="width"/> are invalid.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory2D(Memory<T> memory, int offset, int width, int height)
            : this((ReadOnlyMemory<T>)memory, offset, width, height, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyMemory2D{T}"/> struct.
        /// </summary>
        /// <param name="memory">The target <see cref="Memory{T}"/> to wrap.</param>
        /// <param name="offset">The initial offset within <paramref name="memory"/>.</param>
        /// <param name="width">The width of each row in the resulting 2D area.</param>
        /// <param name="height">The height of the resulting 2D area.</param>
        /// <param name="pitch">The pitch in the resulting 2D area.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="offset"/>, <paramref name="height"/>,
        /// <paramref name="width"/> or <paramref name="pitch"/> are invalid.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory2D(Memory<T> memory, int offset, int width, int height, int pitch)
            : this((ReadOnlyMemory<T>)memory, offset, width, height, pitch)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyMemory2D{T}"/> struct.
        /// </summary>
        /// <param name="memory">The target <see cref="ReadOnlyMemory{T}"/> to wrap.</param>
        /// <param name="offset">The initial offset within <paramref name="memory"/>.</param>
        /// <param name="width">The width of each row in the resulting 2D area.</param>
        /// <param name="height">The height of the resulting 2D area.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="offset"/>, <paramref name="height"/> or <paramref name="width"/> are invalid.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory2D(ReadOnlyMemory<T> memory, int offset, int width, int height)
            : this(memory, offset, width, height, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyMemory2D{T}"/> struct.
        /// </summary>
        /// <param name="memory">The target <see cref="ReadOnlyMemory{T}"/> to wrap.</param>
        /// <param name="offset">The initial offset within <paramref name="memory"/>.</param>
        /// <param name="width">The width of each row in the resulting 2D area.</param>
        /// <param name="height">The height of the resulting 2D area.</param>
        /// <param name="pitch">The pitch in the resulting 2D area.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="offset"/>, <paramref name="height"/>,
        /// <paramref name="width"/> or <paramref name="pitch"/> are invalid.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory2D(ReadOnlyMemory<T> memory, int offset, int width, int height, int pitch)
        {
            if ((uint)offset >= (uint)memory.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForOffset();
            }

            int remaining = memory.Length - offset;

            if ((((uint)width + (uint)pitch) * (uint)height) > (uint)remaining)
            {
                ThrowHelper.ThrowArgumentException();
            }

            this.instance = memory.Slice(offset);
            this.offset = default;
            this.height = height;
            this.width = width;
            this.pitch = pitch;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyMemory2D{T}"/> struct with the specified parameters.
        /// </summary>
        /// <param name="instance">The target <see cref="object"/> instance.</param>
        /// <param name="offset">The initial offset within <see cref="instance"/>.</param>
        /// <param name="height">The height of the 2D memory area to map.</param>
        /// <param name="width">The width of the 2D memory area to map.</param>
        /// <param name="pitch">The pitch of the 2D memory area to map.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlyMemory2D(object instance, IntPtr offset, int height, int width, int pitch)
        {
            this.instance = instance;
            this.offset = offset;
            this.height = height;
            this.width = width;
            this.pitch = pitch;
        }

        /// <summary>
        /// Gets an empty <see cref="ReadOnlyMemory2D{T}"/> instance.
        /// </summary>
        public static ReadOnlyMemory2D<T> Empty => default;

        /// <summary>
        /// Gets a value indicating whether the current <see cref="ReadOnlyMemory2D{T}"/> instance is empty.
        /// </summary>
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (this.height | this.width) == 0;
        }

        /// <summary>
        /// Gets the length of the current <see cref="ReadOnlyMemory2D{T}"/> instance.
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.height * this.width;
        }

        /// <summary>
        /// Gets a <see cref="ReadOnlySpan2D{T}"/> instance from the current memory.
        /// </summary>
        public ReadOnlySpan2D<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!(this.instance is null))
                {
#if SPAN_RUNTIME_SUPPORT
                    if (this.instance.GetType() == typeof(ReadOnlyMemory<T>))
                    {
                        ReadOnlyMemory<T> memory = (ReadOnlyMemory<T>)this.instance;

                        // If the wrapped object is a ReadOnlyMemory<T>, it is always pre-offset
                        ref T r0 = ref memory.Span.DangerousGetReference();

                        return new ReadOnlySpan2D<T>(ref r0, this.height, this.width, this.pitch);
                    }
                    else
                    {
                        // The only other possible cases is with the instance being an array
                        ref T r0 = ref this.instance.DangerousGetObjectDataReferenceAt<T>(this.offset);

                        return new ReadOnlySpan2D<T>(ref r0, this.height, this.width, this.pitch);
                    }
#else
                    return new ReadOnlySpan2D<T>(this.instance, this.offset, this.height, this.width, this.pitch);
#endif
                }

                return default;
            }
        }

        /// <summary>
        /// Slices the current instance with the specified parameters.
        /// </summary>
        /// <param name="row">The target row to map within the current instance.</param>
        /// <param name="column">The target column to map within the current instance.</param>
        /// <param name="width">The width to map within the current instance.</param>
        /// <param name="height">The height to map within the current instance.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="height"/>, <paramref name="width"/> or <paramref name="height"/>
        /// are negative or not within the bounds that are valid for the current instance.
        /// </exception>
        /// <returns>A new <see cref="Memory2D{T}"/> instance representing a slice of the current one.</returns>
        [Pure]
        public unsafe ReadOnlyMemory2D<T> Slice(int row, int column, int width, int height)
        {
            if ((uint)row >= this.height)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForRow();
            }

            if ((uint)column >= this.width)
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForColumn();
            }

            if ((uint)width > (this.width - column))
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForWidth();
            }

            if ((uint)height > (this.height - row))
            {
                ThrowHelper.ThrowArgumentOutOfRangeExceptionForHeight();
            }

            int shift = ((this.width + this.pitch) * row) + column;
            IntPtr offset = (IntPtr)((byte*)this.offset + shift);

            return new ReadOnlyMemory2D<T>(this.instance!, offset, height, width, this.pitch);
        }

        /// <summary>
        /// Copies the contents of this <see cref="Memory2D{T}"/> into a destination <see cref="Memory{T}"/> instance.
        /// </summary>
        /// <param name="destination">The destination <see cref="Memory{T}"/> instance.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="destination" /> is shorter than the source <see cref="Memory2D{T}"/> instance.
        /// </exception>
        public void CopyTo(Memory<T> destination) => Span.CopyTo(destination.Span);

        /// <summary>
        /// Attempts to copy the current <see cref="Memory2D{T}"/> instance to a destination <see cref="Memory{T}"/>.
        /// </summary>
        /// <param name="destination">The target <see cref="Memory{T}"/> of the copy operation.</param>
        /// <returns>Whether or not the operaation was successful.</returns>
        public bool TryCopyTo(Memory<T> destination) => Span.TryCopyTo(destination.Span);

        /// <summary>
        /// Copies the contents of this <see cref="Memory2D{T}"/> into a destination <see cref="Memory2D{T}"/> instance.
        /// For this API to succeed, the target <see cref="Span2D{T}"/> has to have the same shape as the current one.
        /// </summary>
        /// <param name="destination">The destination <see cref="Memory2D{T}"/> instance.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="destination" /> is shorter than the source <see cref="Memory2D{T}"/> instance.
        /// </exception>
        public void CopyTo(Memory2D<T> destination) => Span.CopyTo(destination.Span);

        /// <summary>
        /// Attempts to copy the current <see cref="Memory2D{T}"/> instance to a destination <see cref="Memory2D{T}"/>.
        /// For this API to succeed, the target <see cref="Span2D{T}"/> has to have the same shape as the current one.
        /// </summary>
        /// <param name="destination">The target <see cref="Memory2D{T}"/> of the copy operation.</param>
        /// <returns>Whether or not the operaation was successful.</returns>
        public bool TryCopyTo(Memory2D<T> destination) => Span.TryCopyTo(destination.Span);

        /// <summary>
        /// Creates a handle for the memory.
        /// The GC will not move the memory until the returned <see cref="MemoryHandle"/>
        /// is disposed, enabling taking and using the memory's address.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// An instance with nonprimitive (non-blittable) members cannot be pinned.
        /// </exception>
        /// <returns>A <see cref="MemoryHandle"/> instance wrapping the pinned handle.</returns>
        public unsafe MemoryHandle Pin()
        {
            if (!(this.instance is null))
            {
                GCHandle handle = GCHandle.Alloc(this.instance, GCHandleType.Pinned);

                void* pointer = Unsafe.AsPointer(ref this.instance.DangerousGetObjectDataReferenceAt<T>(this.offset));

                return new MemoryHandle(pointer, handle);
            }

            return default;
        }

        /// <summary>
        /// Tries to get a <see cref="ReadOnlyMemory{T}"/> instance, if the underlying buffer is contiguous.
        /// </summary>
        /// <param name="memory">The resulting <see cref="Memory{T}"/>, in case of success.</param>
        /// <returns>Whether or not <paramref name="memory"/> was correctly assigned.</returns>
        public bool TryGetMemory(out ReadOnlyMemory<T> memory)
        {
            if (this.pitch == 0)
            {
                // Empty Memory2D<T> instance
                if (this.instance is null)
                {
                    memory = default;
                }
                else if (this.instance.GetType() == typeof(ReadOnlyMemory<T>))
                {
                    // If the object is a ReadOnlyMemory<T>, just slice it as needed
                    memory = ((ReadOnlyMemory<T>)this.instance).Slice(0, this.height * this.width);
                }
                else if (this.instance.GetType() == typeof(T[]))
                {
                    // If it's a T[] array, also handle the initial offset
                    memory = new ReadOnlyMemory<T>(Unsafe.As<T[]>(this.instance), (int)this.offset, this.height * this.width);
                }
                else
                {
                    // Reuse a single failure path to reduce
                    // the number of returns in the method
                    goto Failure;
                }

                return true;
            }

            Failure:

            memory = default;

            return false;
        }

        /// <summary>
        /// Copies the contents of the current <see cref="ReadOnlyMemory2D{T}"/> instance into a new 2D array.
        /// </summary>
        /// <returns>A 2D array containing the data in the current <see cref="ReadOnlyMemory2D{T}"/> instance.</returns>
        [Pure]
        public T[,] ToArray() => Span.ToArray();

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj)
        {
            return obj is ReadOnlyMemory2D<T> memory && Equals(memory);
        }

        /// <inheritdoc/>
        public bool Equals(ReadOnlyMemory2D<T> other)
        {
            return
                this.instance == other.instance &&
                this.offset == other.offset &&
                this.height == other.height &&
                this.width == other.width &&
                this.pitch == other.pitch;
        }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            if (!(this.instance is null))
            {
#if SPAN_RUNTIME_SUPPORT
                return HashCode.Combine(
                    RuntimeHelpers.GetHashCode(this.instance),
                    this.offset,
                    this.height,
                    this.width,
                    this.pitch);
#else
                Span<int> values = stackalloc int[]
                {
                    RuntimeHelpers.GetHashCode(this.instance),
                    this.offset.GetHashCode(),
                    this.height,
                    this.width,
                    this.pitch
                };

                return values.GetDjb2HashCode();
#endif
            }

            return 0;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Microsoft.Toolkit.HighPerformance.Memory.ReadOnlyMemory2D<{typeof(T)}>[{this.height}, {this.width}]";
        }

        /// <summary>
        /// Defines an implicit conversion of an array to a <see cref="ReadOnlyMemory2D{T}"/>
        /// </summary>
        public static implicit operator ReadOnlyMemory2D<T>(T[,]? array) => new ReadOnlyMemory2D<T>(array);

        /// <summary>
        /// Defines an implicit conversion of a <see cref="Memory2D{T}"/> to a <see cref="ReadOnlyMemory2D{T}"/>
        /// </summary>
        public static implicit operator ReadOnlyMemory2D<T>(Memory2D<T> memory) => Unsafe.As<Memory2D<T>, ReadOnlyMemory2D<T>>(ref memory);
    }
}
