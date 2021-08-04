namespace Simple.Dotnet.Utilities.Arc
{
    using System;
    using System.Threading;

    public sealed class Arc<T> where T : class
    {
        Action<T>? _drop;
        volatile T? _value;
        int _references = 0;

        public Arc(T value) : this(value, default!) {}

        public Arc(T value, Action<T> drop)
        {
            _value = value;
            _drop = drop;
        }

        public ArcRent<T> Rent()
        {
            if (_value == default) throw new InvalidOperationException("Arc does not contain a value");
            Interlocked.Increment(ref _references);
            return new ArcRent<T>(this);
        }

        void Drop()
        {
            if (Interlocked.Decrement(ref _references) != 0) return;
            _drop?.Invoke(_value!);
            (_value as IDisposable)?.Dispose();
            _value = default;
        }

        public struct ArcRent<T> : IDisposable where T : class
        {
            Arc<T>? _owner;

            public ArcRent(Arc<T> owner)
            {
                _owner = owner;
                Value = owner._value;
            }

            public T? Value { get; private set; }

            public void Dispose()
            {
                Value = default;
                _owner?.Drop();
                _owner = default;
            }
        }
    }
}
