namespace Simple.Dotnet.Utilities.Tasks
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

#if !NETSTANDARD1_6
    using Pools;
    using Microsoft.Extensions.ObjectPool;
#endif

    public sealed class TaskBuffer : IBufferWriter<Task>
    {
        readonly Task[] _tasks;

        public TaskBuffer(int size)
        {
            _tasks = size <= 0 ? Array.Empty<Task>() : new Task[size];
            for (var i = 0; i < _tasks.Length; i++) _tasks[i] = Task.CompletedTask;
        }

        public int Written { get; private set; }
        
        public int Length => _tasks.Length;
        public bool HasSome => Written > 0;
        public int Available => _tasks.Length - Written;
        public bool IsFull => Written == _tasks.Length;

        public ReadOnlySpan<Task> WrittenSpan => new(_tasks, 0, Written);
        public ReadOnlyMemory<Task> WrittenMemory => new(_tasks, 0, Written);
        public ArraySegment<Task> WrittenSegment => new(_tasks, 0, Written);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            if (count < 0) throw new InvalidOperationException($"Can't advance using value {count}");
            if (Written + count > _tasks.Length) throw new InvalidOperationException($"Can't advance, count is too large. Length: {_tasks.Length}. Count: {count}. Offset: {Written}");
            Written += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<Task> GetMemory(int sizeHint = 0)
        {
            if (sizeHint == 0) return new(_tasks, Written, _tasks.Length - Written);
            if (sizeHint < 0) throw new InvalidOperationException($"Can't get memory of size {sizeHint}");
            if (Written + sizeHint > _tasks.Length) throw new InvalidOperationException($"Can't get memory of hint: {sizeHint}. Length: {_tasks.Length}. Offset: {Written}");

            return new(_tasks, Written, sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<Task> GetSpan(int sizeHint = 0)
        {
            if (sizeHint == 0) return new(_tasks, Written, _tasks.Length - Written);
            if (sizeHint < 0) throw new InvalidOperationException($"Can't get span of size {sizeHint}");
            if (Written + sizeHint > _tasks.Length) throw new InvalidOperationException($"Can't get span of hint: {sizeHint}. Length: {_tasks.Length}. Offset: {Written}");

            return new(_tasks, Written, sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            for (var i = 0; i < Written; i++) _tasks[i] = Task.CompletedTask;
            Written = 0;
        }

        public static implicit operator Task[](TaskBuffer buffer) => buffer._tasks;
    }

    public sealed class TaskBuffer<T> : IBufferWriter<Task<T>>
    {
        static readonly Task<T?> Completed = Task.FromResult(default(T));

        Task<T>[] _tasks;

        public TaskBuffer(int size)
        {
            _tasks = size <= 0 ? Array.Empty<Task<T>>() : new Task<T>[size];
            for (var i = 0; i < _tasks.Length; i++) _tasks[i] = Completed;
        }

        public int Written { get; private set; }

        public int Length => _tasks.Length;
        public bool HasSome => Written > 0;
        public int Available => _tasks.Length - Written;
        public bool IsFull => Written == _tasks.Length;

        public ReadOnlySpan<Task<T>> WrittenSpan => new(_tasks, 0, Written);
        public ReadOnlyMemory<Task<T>> WrittenMemory => new(_tasks, 0, Written);
        public ArraySegment<Task<T>> WrittenSegment => new(_tasks, 0, Written);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            if (count < 0) throw new InvalidOperationException($"Can't advance using value {count}");
            if (Written + count > _tasks.Length) throw new InvalidOperationException($"Can't advance, count is too large. Length: {_tasks.Length}. Count: {count}. Offset: {Written}");
            Written += count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<Task<T>> GetMemory(int sizeHint = 0)
        {
            if (sizeHint == 0) return new(_tasks, Written, _tasks.Length - Written);
            if (sizeHint < 0) throw new InvalidOperationException($"Can't get memory of size {sizeHint}");
            if (Written + sizeHint > _tasks.Length) throw new InvalidOperationException($"Can't get memory of hint: {sizeHint}. Length: {_tasks.Length}. Offset: {Written}");

            return new(_tasks, Written, sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<Task<T>> GetSpan(int sizeHint = 0)
        {
            if (sizeHint == 0) return new(_tasks, Written, _tasks.Length - Written);
            if (sizeHint < 0) throw new InvalidOperationException($"Can't get memory of size {sizeHint}");
            if (Written + sizeHint > _tasks.Length) throw new InvalidOperationException($"Can't get memory of hint: {sizeHint}. Length: {_tasks.Length}. Offset: {Written}");

            return new(_tasks, Written, sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            for (var i = 0; i < Written; i++) _tasks[i] = Completed;
            Written = 0;
        }

        public static implicit operator Task[](TaskBuffer<T> buffer) => Unsafe.As<Task<T>[], Task[]>(ref buffer._tasks);
        public static implicit operator Task<T>[](TaskBuffer<T> buffer) => buffer._tasks;
    }

#if !NETSTANDARD1_6

    public sealed class TaskBufferPool : IPool<TaskBuffer>
    {
        readonly ObjectPool<TaskBuffer> _pool;

        public TaskBufferPool(int capacity) => _pool = new DefaultObjectPool<TaskBuffer>(new CapacityPolicy(capacity));
        public TaskBufferPool(int capacity, int poolSize) => _pool = new DefaultObjectPool<TaskBuffer>(new CapacityPolicy(capacity), poolSize);

        public ObjectRent<TaskBuffer> Get() => new(_pool, b => b.Clear());

        sealed class CapacityPolicy : PooledObjectPolicy<TaskBuffer>
        {
            readonly int _capacity;

            public CapacityPolicy(int capacity) => _capacity = capacity;

            public override TaskBuffer Create() => new(_capacity);

            public override bool Return(TaskBuffer obj) => true;
        }
    }

    public sealed class TaskBufferPool<T>: IPool<TaskBuffer<T>>
    {
        readonly ObjectPool<TaskBuffer<T>> _pool;

        public TaskBufferPool(int capacity) => _pool = new DefaultObjectPool<TaskBuffer<T>>(new CapacityPolicy(capacity));
        public TaskBufferPool(int capacity, int poolSize) => _pool = new DefaultObjectPool<TaskBuffer<T>>(new CapacityPolicy(capacity), poolSize);

        public ObjectRent<TaskBuffer<T>> Get() => new(_pool, b => b.Clear());

        sealed class CapacityPolicy : PooledObjectPolicy<TaskBuffer<T>>
        {
            readonly int _capacity;

            public CapacityPolicy(int capacity) => _capacity = capacity;

            public override TaskBuffer<T> Create() => new(_capacity);

            public override bool Return(TaskBuffer<T> obj) => true;
        }
    }

#endif

    public static class TaskBufferExtensions
    {
        public static IEnumerable<Task> AsEnumerable(this TaskBuffer writer)
        {
            Task[] tasks = writer;
            for (var i = 0; i < writer.Written; i++) yield return tasks[i];
        }

        public static IEnumerable<Task<T>> AsEnumerable<T>(this TaskBuffer writer)
        {
            Task[] tasks = writer;
            for (var i = 0; i < writer.Written; i++) yield return (Task<T>)tasks[i];
        }

        public static IEnumerable<Task<T>> AsEnumerable<T>(this TaskBuffer<T> writer)
        {
            Task<T>[] tasks = writer;
            for (var i = 0; i < writer.Written; i++) yield return tasks[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(this TaskBuffer writer, Task data)
        {
            writer.GetSpan(1)[0] = data;
            writer.Advance(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append<T>(this TaskBuffer writer, Task<T> data)
        {
            writer.GetSpan(1)[0] = data;
            writer.Advance(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append<T>(this TaskBuffer<T> writer, Task<T> data)
        {
            writer.GetSpan(1)[0] = data;
            writer.Advance(1);
        }
    }
}
