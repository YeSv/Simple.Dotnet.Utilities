namespace Simple.Dotnet.Utilities.Buffers
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public struct Rent<T> : IDisposable, IBufferWriter<T>
    {
        T[] _array;

        public Rent(int size)
        {
            Written = 0;
            Length = size < 0 ? 0 : size;
            _array = Length > 0 ? ArrayPool<T>.Shared.Rent(Length) : Array.Empty<T>();
        }

        public int Length { get; private set; }
        public int Written { get; private set; }

        public bool HasSome => Written > 0;
        public bool IsFull => Written == Length;
        public int Available => Length - Written;

        public ReadOnlySpan<T> WrittenSpan => new (_array, 0, Written);
        public ReadOnlyMemory<T> WrittenMemory => new (_array, 0, Written);
        public ArraySegment<T> WrittenSegment => new (_array, 0, Written);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(T data)
        {
            GetSpan(1)[0] = data;
            Advance(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            if (count < 0) throw new InvalidOperationException($"Can't advance using value {count}");
            if (Written + count > Length) throw new InvalidOperationException($"Can't advance, count is too large. Length: {Length}. Count: {count}. Offset: {Written}");
            Written += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> GetMemory(int sizeHint = 0)
        {
            if (sizeHint == 0) return new (_array, Written, Length - Written);
            if (sizeHint < 0) throw new InvalidOperationException($"Can't get memory of size {sizeHint}");
            if (Written + sizeHint > Length) throw new InvalidOperationException($"Can't get memory of hint: {sizeHint}. Length: {Length}. Offset: {Written}");

            return new(_array, Written, sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetSpan(int sizeHint = 0)
        {
            if (sizeHint == 0) return new(_array, Written, Length - Written);
            if (sizeHint < 0) throw new InvalidOperationException($"Can't get span of size {sizeHint}");
            if (Written + sizeHint > Length) throw new InvalidOperationException($"Can't get span of hint: {sizeHint}. Length: {Length}. Offset: {Written}");

            return new(_array, Written, sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Array.Clear(_array, 0, Written);
            Written = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            if (Length > 0) ArrayPool<T>.Shared.Return(_array, true);

            Length = 0;
            Written = 0;
            _array = Array.Empty<T>();
        }
    }

    public static class RentExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this Rent<T> rent)
        {
            var memory = rent.WrittenMemory;
            for (var i = 0; i < memory.Length; i++) yield return memory.Span[i];
        }

        public static void CopyTo<T>(this Rent<T> rent, IBufferWriter<T> writer) => rent.WrittenSpan.CopyTo(writer.GetSpan(rent.WrittenSpan.Length));
    }
}
