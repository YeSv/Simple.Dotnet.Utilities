namespace Simple.Dotnet.Utilities
{
    using System;
    using System.Runtime.CompilerServices;

    public ref struct ArrayStack<T> where T : unmanaged
    {
        int _index;
        readonly Span<T> _span;

        public ArrayStack(Span<T> span)
        {
            _index = 0;
            _span = span;
        }

        public int Length => _span.Length;
        public bool IsFull => _index == _span.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop() => _span[--_index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T v) => _span[_index++] = v;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _index = 0;

        public ReadOnlySpan<T> WrittenSpan => _index == 0 ? ReadOnlySpan<T>.Empty : _span.Slice(0, _index);
    }
}
