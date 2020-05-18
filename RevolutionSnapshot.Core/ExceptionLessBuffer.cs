// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace RevolutionSnapshot.Core
{
    /// <summary>
    /// Represents a heap-based, array-backed output sink into which <typeparam name="T"/> data can be written.
    /// </summary>
    public sealed class ExceptionLessBuffer<T> : IBufferWriter<T>
    {
        private T[] _buffer;
        private int _index;

        private const int DefaultInitialBufferSize = 256;

        public ExceptionLessBuffer(int initialCapacity)
        {
            if (initialCapacity <= 0)
                throw new ArgumentException(nameof(initialCapacity));

            _buffer = new T[initialCapacity];
            _index = 0;
        }

        public ReadOnlyMemory<T> WrittenMemory => _buffer.AsMemory(0, _index);

        public ReadOnlySpan<T> WrittenSpan => _buffer.AsSpan(0, _index);
        
        public int WrittenCount => _index;
        
        public int Capacity => _buffer.Length;
        
       
        public int FreeCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _buffer.Length - _index; }
        }

        public void Clear()
        {
            _buffer.AsSpan(0, _index).Clear();
            _index = 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            _index += count;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> GetMemory(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            return _buffer.AsMemory(_index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetSpan(int sizeHint = 0)
        {
            CheckAndResizeBuffer(sizeHint);
            return _buffer.AsSpan(_index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckAndResizeBuffer(int sizeHint)
        {
            if (sizeHint == 0)
            {
                sizeHint = 1;
            }

            if (sizeHint > FreeCapacity)
            {
                var growBy = Math.Max(sizeHint, _buffer.Length);

                if (_buffer.Length == 0)
                {
                    growBy = Math.Max(growBy, DefaultInitialBufferSize);
                }

                int newSize = checked(_buffer.Length + growBy);

                Array.Resize(ref _buffer, newSize);
            }
        }
    }
}
