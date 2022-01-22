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
        Action<T>? _returnPolicy;

        public ObjectRent(ObjectPool<T> pool) : this(pool, null)
        { }

        public ObjectRent(ObjectPool<T> pool, Action<T>? returnPolicy) =>
            (_pool, _value, _returnPolicy) = (pool, default, returnPolicy);

        public T Value => _value ??= _pool?.Get()!;

        public void Dispose()
        {
            var (value, pool, policy) = (_value, _pool, _returnPolicy);

            (_value, _pool, _returnPolicy) = (null, null, null);

            if (value == null) return;

            policy?.Invoke(value);
            pool?.Return(value);
        }
    }
}

#endif
