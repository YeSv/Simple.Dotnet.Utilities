namespace Simple.Dotnet.Utilities.Buffers
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public sealed class ArrayBufferWriter<T> : IBufferWriter<T>
    {
        readonly T[] _array;

        public ArrayBufferWriter(T[] array) => _array = array;

        public ArrayBufferWriter(T[] array, T @default)
        {
            _array = array;
            for (var i = 0; i < _array.Length; i++) _array[i] = @default;
        }

        public ArrayBufferWriter(int size) => _array = size <= 0 ? Array.Empty<T>() : new T[size];

        public ArrayBufferWriter(int size, T @default)
        {
            _array = size <= 0 ? Array.Empty<T>() : new T[size];
            for (var i = 0; i < _array.Length; i++) _array[i] = @default;
        }

        public int Written { get; private set; }

        public int Length => _array.Length;
        public bool HasSome => Written > 0;
        public int Available => _array.Length - Written;
        public bool IsFull => Written == _array.Length;

        public ReadOnlySpan<T> WrittenSpan => new(_array, 0, Written);
        public ReadOnlyMemory<T> WrittenMemory => new(_array, 0, Written);
        public ArraySegment<T> WrittenSegment => new(_array, 0, Written);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            if (count < 0) throw new InvalidOperationException($"Can't advance using value {count}");
            if (Written + count > _array.Length) throw new InvalidOperationException($"Can't advance, count is too large. Length: {_array.Length}. Count: {count}. Offset: {Written}");
            Written += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> GetMemory(int sizeHint = 0)
        {
            if (sizeHint == 0) return new(_array, Written, _array.Length - Written);
            if (sizeHint < 0) throw new InvalidOperationException($"Can't get memory of size {sizeHint}");
            if (Written + sizeHint > _array.Length) throw new InvalidOperationException($"Can't get memory of hint: {sizeHint}. Length: {_array.Length}. Offset: {Written}");

            return new(_array, Written, sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetSpan(int sizeHint = 0)
        {
            if (sizeHint == 0) return new(_array, Written, _array.Length - Written);
            if (sizeHint < 0) throw new InvalidOperationException($"Can't get span of size {sizeHint}");
            if (Written + sizeHint > _array.Length) throw new InvalidOperationException($"Can't get span of hint: {sizeHint}. Length: {_array.Length}. Offset: {Written}");

            return new(_array, Written, sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(_array, 0, Written);
            Written = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear(T @default)
        {
            Clear();
            for (var i = 0; i < _array.Length; i++) _array[i] = @default;
        }
    }


    public static class ArrayBufferWriterExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this ArrayBufferWriter<T> writer)
        {
            var memory = writer.WrittenMemory;
            for (var i = 0; i < memory.Length; i++) yield return memory.Span[i];
        }

        public static void CopyTo<T>(this ArrayBufferWriter<T> source, IBufferWriter<T> destination) => source.WrittenSpan.CopyTo(destination.GetSpan(source.Written));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append<T>(this ArrayBufferWriter<T> writer, T data)
        {
            writer.GetSpan(1)[0] = data;
            writer.Advance(1);
        }
    }
}
