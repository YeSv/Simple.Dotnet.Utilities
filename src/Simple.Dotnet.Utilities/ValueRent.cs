namespace Simple.Dotnet.Utilities.Pools
{
    using System;
    
    public struct ValueRent<T, TContext> : IDisposable
    {
        T? _value;
        TContext? _context;
        Action<T, TContext?>? _returnPolicy;

        public ValueRent(T value, Action<T, TContext?> returnPolicy) : this(value, default, returnPolicy) { }

        public ValueRent(T value, TContext? context, Action<T, TContext?> returnPolicy)
        {
            _value = value;
            _context = context;
            _returnPolicy = returnPolicy;
        }
        
        public T Value => _value!;

        public void Dispose()
        {
            var value = _value; var context = _context; var policy = _returnPolicy;

            _value = default;
            _context = default;
            _returnPolicy = default;

            policy?.Invoke(value!, context);
        }
    }

    public struct ValueRent<T> : IDisposable
    {
        T? _value;
        object? _context;
        Action<T, object?>? _returnPolicy;

        public ValueRent(T value, Action<T, object?> returnPolicy) : this(value, default, returnPolicy) { }

        public ValueRent(T? value, object? context, Action<T, object?>? returnPolicy)
        {
            _value = value;
            _context = context;
            _returnPolicy = returnPolicy;
        }

        public T Value => _value!;

        public void Dispose()
        {
            var value = _value; var context = _context; var policy = _returnPolicy;

            _value = default;
            _context = default;
            _returnPolicy = default;

            policy?.Invoke(value!, context);
        }
    }
}
