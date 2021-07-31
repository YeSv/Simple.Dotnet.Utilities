namespace Simple.Dotnet.Utilities.Buffers
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

#if !NETSTANDARD1_6
    using Microsoft.Extensions.ObjectPool;
    using Pools;
#endif

    public sealed class BufferWriter<T> : IBufferWriter<T>, IDisposable
    {
        static readonly int DefaultCapacity = 10;

        Rent<T> _rent;
        readonly int _capacity;

        public BufferWriter() : this(DefaultCapacity) { }

        public BufferWriter(int capacity)
        {
            _capacity = capacity;
            _rent = new(capacity);
        }

        public ReadOnlySpan<T> WrittenSpan => _rent.WrittenSpan;
        public ReadOnlyMemory<T> WrittenMemory => _rent.WrittenMemory;
        public ArraySegment<T> WrittenSegment => _rent.WrittenSegment;
        
        public bool HasSome => _rent.HasSome;
        public int Written => _rent.WrittenSpan.Length;

        void Grow(int sizeHint)
        {
            var growSize = Math.Max(sizeHint + _rent.Written, _rent.Written * 2);
            
            var oldRent = _rent;
            _rent = new Rent<T>(growSize);

            oldRent.WrittenSpan.CopyTo(_rent.GetSpan(oldRent.Written));
            _rent.Advance(oldRent.Written);

            oldRent.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count) => _rent.Advance(count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> GetMemory(int sizeHint = 0)
        {
            if (_rent.IsFull || _rent.Available < sizeHint) Grow(sizeHint);
            return _rent.GetMemory(sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetSpan(int sizeHint = 0)
        {
            if (_rent.IsFull || _rent.Available < sizeHint) Grow(sizeHint);
            return _rent.GetSpan(sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _rent.Dispose();
            _rent = new(_capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _rent.Dispose();
    }


#if !NETSTANDARD1_6

    public sealed class BufferWriterPool<T> : IPool<BufferWriter<T>>
    {
        static readonly int DefaultCapacity = 10;
        public static readonly BufferWriterPool<T> Shared = new ();

        readonly ObjectPool<BufferWriter<T>> _pool;

        public BufferWriterPool() : this(DefaultCapacity) {}
        public BufferWriterPool(int arrayCapacity) => _pool = new DefaultObjectPool<BufferWriter<T>>(new CapacityPolicy(arrayCapacity), Environment.ProcessorCount * 3);
        public BufferWriterPool(int arrayCapacity, int poolSize) => _pool = new DefaultObjectPool<BufferWriter<T>>(new CapacityPolicy(arrayCapacity), poolSize);

        public ObjectRent<BufferWriter<T>> Get() => new (_pool, r => r.Clear());

        sealed class CapacityPolicy : PooledObjectPolicy<BufferWriter<T>>
        {
            readonly int _capacity;

            public CapacityPolicy(int capacity) => _capacity = capacity;

            public override BufferWriter<T> Create() => new (_capacity);

            public override bool Return(BufferWriter<T> obj) => true;
        }
    }

#endif

    public static class BufferWriterExtensions 
    {
        public static IEnumerable<T> AsEnumerable<T>(this BufferWriter<T> writer)
        {
            var memory = writer.WrittenMemory;
            for (var i = 0; i < memory.Length; i++) yield return memory.Span[i];
        }

        public static void CopyTo<T>(this BufferWriter<T> source, IBufferWriter<T> destination)
        {
            source.WrittenSpan.CopyTo(destination.GetSpan(source.Written));
            destination.Advance(source.Written);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append<T>(this BufferWriter<T> writer, T data)
        {
            writer.GetSpan(1)[0] = data;
            writer.Advance(1);
        }
    }
}
