#if !NETSTANDARD1_6

namespace Simple.Dotnet.Utilities.Pools
{
    using System;
    using Microsoft.Extensions.ObjectPool;

    public interface IPool<T> where T : class
    {
        ObjectRent<T> Get();
    }

    public struct ObjectRent<T> : IDisposable where T: class 
    {
        T? _value;
        ObjectPool<T>? _pool;
        readonly Action<T>? _returnPolicy;

        public ObjectRent(ObjectPool<T> pool) : this(pool, null)
        { }

        public ObjectRent(ObjectPool<T> pool, Action<T>? returnPolicy)
        {
            _pool = pool;
            _value = null;
            _returnPolicy = returnPolicy;
        }

        public T Value => _value ??= _pool?.Get();

        public void Dispose()
        {
            if (_value == null) return;
            
            _returnPolicy?.Invoke(_value);
            _pool?.Return(_value);

            _pool = null;
            _value = null;
        }
    }
}

#endif
