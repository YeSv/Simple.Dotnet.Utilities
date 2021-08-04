namespace Simple.Dotnet.Utilities
{
    using System;
    using System.Runtime.CompilerServices;

    public ref struct ArrayStack<T> where T : unmanaged
    {
        static readonly int Empty = -1;

        int _index;
        Span<T> _span;

        public ArrayStack(Span<T> span)
        {
            _span = span;
            _index = Empty;
        }

        public int Length => _span.Length;
        public bool IsEmpty => _index == Empty;
        public bool IsFull => _index == (_span.Length - 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
            if (_index == Empty) throw new InvalidOperationException("Can't pop an element from an empty stack");
            return _span[_index--];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(T v)
        {
            if (IsFull) throw new InvalidOperationException("Can't push an element to full stack");
            _span[++_index] = v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _index = -1;

        public ReadOnlySpan<T> WrittenSpan => IsEmpty ? ReadOnlySpan<T>.Empty : _span.Slice(0, _index + 1);
    }
}
