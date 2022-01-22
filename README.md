## NuGet
[Simple.Dotnet.Utilities](https://www.nuget.org/packages/Simple.Dotnet.Utilities/)

## Table of contents
1. [Results](#1-results) - Wrappers around return values
2. [Rent](#2-rent) - Allows to use `IDisposable` `using` syntax on an array from `ArrayPool<T>`
3. [TaskBuffer](#3-taskbuffer) - Buffer of `Task`/`Task<T>` for apps that use batching
4. [BufferWriter](#4-bufferwriter) - Resizable implementation of `IBufferWriter<T>`
5. [ArrayBufferWriter](#5-arraybufferwriter) - Fixed-length implementation of `IBufferWriter<T>`
6. [ObjectRent](#6-objectrent) - Allows an object reuse by returning it to `ObjectPool<T>`
7. [Maps](#7-maps) - Useful maps allocated on a stack
8. [Partitioners](#8-partitioners) - Simple partitioners implementations
9. [ArrayStack](#9-arraystack) - A stack data structure that uses a `Span<T>` under the hood
10. [Arc](#10-arc) - Simple atomic reference counting implementation inspired from Rust language
11. [ValueRent](#11-valuerent) - Wrapper around rented value

# 1. Results

Imagine a typical AspNet core application that might write data to some storage. For example, user information:

``` csharp

public record UserInformation(string Name, string LastName, DateTime DateOfBirth);

```

And a storage:

``` csharp

// Pseudocode

public interface IStorage {
    Task<?> Save(UserInformation info);
}

```

Some questions arise with an interface provided above:

What should such method return? - A simple `Task` might do the trick. 
But how should we handle an error? - We can setup a `ILogger` implementation as a private property and log an exception or don't catch an error at all and hope that there is a `try/catch` block somewhere in our app.
How can caller understand if this method handles an error or maybe it never throws an error?

Well, `UniResult<TOk, TError>` and `Result<TOk, TError>` to the rescue:

``` csharp

public readonly struct Result<TOk, TError> : IEquatable<Result<TOk, TError>>
{
    public readonly TOk? Ok;
    public readonly TError? Error;
    public readonly bool IsOk;

    public Result(TOk? ok)
    {
        Ok = ok;
        IsOk = true;
        Error = default;
    }

    public Result(TError error)
    {
        Ok = default;
        Error = error;
        IsOk = false;
    }
    
    // Other methods like Equals/ToString etc
}

public readonly struct UniResult<TOk, TError> : IEquatable<UniResult<TOk, TError>> where TOk : class where TError : class
{
    public readonly object? Data;

    public UniResult(TOk? data) => Data = data;
    public UniResult(TError error) => Data = error;

    public bool IsOk => Data is TOk or null;

    // Other methods like Equals/ToString etc
}

```

As you can see those are `readonly struct`s which means that they live on a stack + they are immutable + they have a copy semantics (they are not passed by reference).
You can store either error result of type `TError` or successful result of type `TOk` - nothing else.
Also all of the fields are public which means that you can write your own extensions freely, a common usage of such structs might be like this:

``` csharp

// Pseudocode

var result = new UniResult<string, Exception>("Hello there");
if (result.IsOk) Console.WriteLine(result.Ok);
else Console.WriteLine(result.Error.ToString());

// Or

var message = result switch 
{
    (var ok, default) => ok,
    (default, var exception) => exception.ToString()
};

Console.WriteLine(message);

// Or

message = result.IsOk switch 
{
    true => result.Ok,
    false => result.Error
};

```

You can use pattern matching to get internal values either error or data, or using `else/if` :)

The difference between `UniResult` and `Result` is that `UniResult` can only contain classes while `Result` can contain structs. `UniResult` only requires a size of a reference on a stack because internally it has only one field of type `object` and casts it to `TOk` or to `TError`, you can use structs with `UniResult` but structs will be boxed, that's why it's better to consider `Result` for such cases. `Result` uses more stack space because it contains three fields - `bool` to determine if result is ok or error and two data fields of types `TOk` and `TError`.

We can now rewrite our `IStorage` definition like this:

``` csharp

public interface IStorage 
{
    Task<UniResult<Unit, Exception>> Save(UserInformation info);
}

```

Notice that we now explicitly state that method returns some class `Unit` or `Exception` so caller understands that any exception is returned and not thrown. Why can this be beneficial? Throwing and unwinding stack can be costly so it's better to catch it and return as is. Also with `Result/UniResult` we can't use something like `UniResult<void, Exception>` that's why `Unit` is provided - which is a class with no fields/properties, which means that you return nothing.

``` csharp

public sealed class Unit : IEquatable<Unit>
{
    public static readonly Unit Shared = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Unit other) => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is Unit other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => nameof(Unit);
}

```

Results are also good for validation or buisness logic, instead of throwing an exception you can return a `Result` instead with error or success result:

``` csharp

// Pseudocode

public UniResult<Unit, string> Validate(UserInformation info) {
    if (string.IsNullOrWhiteSpace(info.FirstName)) return UniResult.Error<Unit, string>("FirstName should not be empty....");
    // other vaidations

    return UniResult.Ok<Unit, string>(Unit.Shared);
}

```

So why would someone use `UniResult/Result`:
1. You don't want to allocate a wrapper
2. You have your errors and ok values as classes so you can benefit by using only one field (for `UniResult`)
3. Your code throws on errors, throwing an exception is not cheap, so you might move to `UniResult/Result` instead
4. You want to use something like this but don't want to write it from scratch
5. You've seen Result in F# or in Rust, this is mostly the same

Here is an example on how you might use `Result` in your app code:

``` csharp

// Pseudocode

public enum Code : byte {
    Ok,
    ValidationError,
    UnhandledError
}

public readonly struct Response<T> {
    public readonly Code Code;
    public readonly T? Data;
    public readonly string? Reason;

    public Response(Code code, T? data, string? reason) => (Code, Data, Reason) = (code, data, reason);

    public Response<T> Success(T data) => new (Code.Ok, data, null);
    public Response<T> Error(Code code, string reason) =>  new (code, null, reason);
}

public readonly struct AppError {
    public readonly Code Code;
    public readonly string? Message;
    public readonly Exception? Exception;

    public AppError(Code code, Exception? ex, string? message) => (Code, Exception, Message) = (code, ex, message);

    public static AppError ValidationError(string message) => new (Code.ValidationError, null, message);

    public static AppError UnhandledException(Exception ex) => new (Code.UnhandledError, ex, ex.Message);
}

// Somewhere: 
public Result<Unit, AppError> Validate(ImportantData data) 
{
    if (data.AmountPaid < 0) return new Result<Unit, AppError>(AppError.ValidationError("AmountPaid < 0"));

    return new Result<Unit, AppError>(Unit.Shared); // or Result.Ok(new Unit())
}

// Response from other service

public struct ServiceResponse 
{
    public string Code {get; set; } // For deserializators
    public bool IsOk { get; set; }

    public ServiceResponse(string code, bool isOk) => (Code, IsOk) = (code, isOk);
}


// Somewhere else http client that calls other service
public async Task<Result<ServiceResponse, AppError>> Get(ImportantData data) {
    try {
        var response = await _http.PostAsJsonAsync("{URL}", data);
        var serviceResponse = await response.ReadAsJsonAsync<ServiceResponse>();

        return new Result<ServiceResponse, AppError>(serviceResponse);
    }
    catch (Exception ex) {
        return new Result<ServiceResponse, AppError>(AppError.UnhandledException(ex));
    }

}

// Somwehere else
public async Task<Result<ServiceResponse, AppError>> DoStuff(ImportantData data) 
{
    var validationResult = Validate(data);

    // Should recast from Result<Unit, AppError> to Result<ServiceResponse, AppError>
    if (!validationResult.IsOk) return new Result<ServiceResponse, AppError>(validationResult.Error);

    var result = await Get(data);
    return result; // The same type can just return
}

public static class Extensions {
    public static Response ToResponse<T>(this in Result<T, AppError> result, ILogger logger) {
        if (result.IsOk) return Response.Ok(result.Data);

        if (result.Error.Code == Code.ValidationError) {
            logger.Error($"Validation error occurred. Reason: {result.Error.Message}");
            return Response.Error(Code.ValidationError, result.Error.Message);
        }

        logger.Error(result.Error.Exception!, "Unhandled exception occurred");
        return Response.Error(Code.UnhandlerException, "Unhandled exception occurred");
    }
}

```

Here is the simple application code, we send http requests and handle exceptions, have an extension to write logs and also have a validation mechanism, everything is plain simple. Rule of thumb - you should handle an exception in a method that returns a ``Result`` or ``UniResult``, this way methods like ``DoStuff`` can rely that if inner method return results everything is handled.

__NOTES__:
1. Why all fields are public? Well Results are ``readonly struct``s, so you can't exchange data unless you use non-imutable data like ``Dictionary<TKey, TValue>``, also you gain a benefit to write your own extensions by your needs as all fields are available to you, pretty simple.
2. You can replace ``Result<TOk, TError>`` with ``ValueTuple<TOk, TError>`` like guys in Go do, this is your choice, you can also build lots of extensions on top of such tuples, it's up to you on what to use)
3. Why should I use Results if I can use ``language-ext`` library? ``language-ext`` is perfect library, it has ``Option``, ``Either``, ``Unit``, and lots of features, but it relies on functional stuff, notice ``Simple`` in the name of package :) You can download it and use C#-like style code. That's the goal, + it's pretty simple to understand only three classes that have 1-3 fields :D


# 2. Rent

You've probably heard about ``ArrayPool<T>`` class. Using this class you can rent an array of specified size (or bigger) from shared pool or create your own.

Usage example:

Suppose you have a queue that worker thread tries to read and write data into a batch, and then other code processes a batch, you don't know how many items are in queue, so allocating an array of predefined batch size might be too expensive, for example, batch size is 1000, but queue contains only 100 elements, possible solution is to use array pool, rent an array with the size of a batch and then allocate an array of required size, example:

``` csharp

// Pseudocode

public T[] GetBatch(ConcurrentQueue<T> queue, int batchSize) {
    var array = ArrayPool<T>.Shared.Rent(batchSize);
    var written = 0;

    try {
        while (written < batchSize && queue.TryDequeue(out var item)) array[written++] = item;
        return array.AsSpan(0, written).ToArray();
    }
    finally {
        ArrayPool<T>.Shared.Return(array);
    }
}

```

This is a simple example but you can see that you might forget to return an array to the pool + you have to track a size because array may have larger length. Also we allocate an array, because we can't return pooled array as someone else doesn't know that it is pooled. Well, ``Rent<T>`` to the rescue. 

``Rent<T>`` is a ``struct`` that manages array from a pool, and returns an array once you're done using it. It also manages length, and implements ``IBufferWriter<T>``.

``` csharp

public struct Rent<T> : IDisposable, IBufferWriter<T>
{
    readonly int _size;
    readonly T[] _array;

    public Rent(int size)
    {
        _size = size;
        Written = 0;
        _array = _size > 0 ? ArrayPool<T>.Shared.Rent(size) : Array.Empty<T>();
    }

    public int Written { get; private set; }

    public bool HasSome => Written > 0;
    public bool IsFull => Written == _size;
    public int Available => _size - Written;

    // other code
}

``` 

As you can see it's a ``IDisposable`` implementation, so on a ``Dispose`` call array is returned to the pool. Let's rewrite previous method with ``Rent<T>``:

``` csharp

// Pseudocode

// Implementation where we can return a Rent, so caller can dispose it
public Rent<T> GetBatch(ConcurrentQueue<T> queue, int batchSize) {
    var rent = new Rent<T>(batchSize);
    while (!rent.IsFull && queue.TryDequeue(out var item)) rent.Append();
    return rent;
}

// The same implementation where we return Rent, we only need to use using
public T[] GetBatch(ConcurrentQueue<T> queue, int batchSize) {
    using var rent = new Rent<T>(batchSize);
    while (!rent.IsFull && queue.TryDequeue(out var item)) rent.Append();
    return rent.WrittenSpan.ToArray();
}

// alternative approach using IBufferWriter<T>
public T[] GetBatch(ConcurrentQueue<T> queue, int batchSize) {
    using var rent = new Rent<T>(batchSize);
    
    var written = 0;
    var span = rent.GetSpan(batchSize);

    while (written < batchSize && queue.TryDequeue(out var item)) span[written++] = item;

    rent.Advance(written);

    return rent.WrittenSpan.ToArray();
}

```

You can use ``Rent<T>`` as ``IBufferWriter<T>`` with all the methods this interface provides like ``GetSpan``, ``GetMemory`` and ``Advance``. ``Rent<T>`` also provides ``WrittenSpan`` ``WrittenMemory`` and ``WrittenSegment`` (array segment), you can also ``Clear`` rent to reuse it  (internal index is set to 0 and array's content is cleared). You can also make it ``IEnumerable<T>`` using ``AsEnumerable`` extension, note that you still have to dispose it :).

# 3. TaskBuffer

``TaskBuffer`` (or a ``FutureBuffer``) is an implementation of ``IBufferWriter<Task>`` or ``IBufferWriter<Task<T>>``. It's only intended to be used with ``Task``s.

Mostly, the only reason to use it is when you have a batch that you should process in parallel, let's check this example:

From (2) we have a method that can get batches of ``batchSize``, now imagine that for each element in batch you have to send HttpRequest, let's write a method that can do just like this:

 ``` csharp

// Pseudocode

var batchSize = 1_000;
var client = new HttpClient("someapi.com");

// Emulate a thread that reads batches and sends data to HttpClient
while (true) 
{
    using var rent = GetBatch(queue, batchSize); // GetBatch method from section (2), returns Rent<T>.

    var writtenMemory = rent.WrittenMemory;
    for (var i = 0; i < writtenMemory.Length; i++) await http.PostAsJsonAsync(writtenMemory.Span[i]);

    // Do other stuff
}

 ```

Once we have a batch, we can simply send ``HTTP`` ``POST`` requests one by one to our destination.
We can do better, why can't we just send batches using ``Task.WhenAll()``, unfortunatelly, batches may vary in size, so we have to allocate new array each time.

``` csharp

// Pseudocode

var batchSize = 1_000;
var client = new HttpClient("someapi.com");

// Emulate a thread that reads batches and sends data to HttpClient
while (true) 
{
    using var rent = GetBatch(queue, batchSize); // GetBatch method from section (2), returns Rent<T>.

    var writtenMemory = rent.WrittenMemory;
    var tasks = Task<HttpResponseMessage>[writtenMemory.Length];
    for (var i = 0; i < writtenMemory.Length; i++) tasks[i] = http.PostAsJsonAsync(writtenMemory.Span[i]);

    await Task.WhenAll(tasks); // wait for all requests
    // Do other stuff
}

```

Well, we allocate new array each time we receive a batch. 
What we can do instead is to pre-allocate an array of ``batchSize`` and set all elements to ``Task.CompletedTask``. Once we are done with all requests we can kind of "reset" a buffer with ``Task.CompletedTask`` once more or ``Task.FromResult((HttpResponseMessage)null)`` for our case.
Let's try this:

``` csharp

// Pseudocode

var batchSize = 1_000;
var client = new HttpClient("someapi.com");
var fromResult = Task.FromResult((HttpResponseMessage)null);
var taskBuffer = new Task<HttpResponseMessage>[batchSize];
for (var i = 0; i < taskBuffer.Length; i++) taskBuffer[i] = fromResult;

// Emulate a thread that reads batches and sends data to HttpClient
while (true) 
{
    using var rent = GetBatch(queue, batchSize); // GetBatch method from section (2), returns Rent<T>.

    var writtenMemory = rent.WrittenMemory;
    for (var i = 0; i < writtenMemory.Length; i++) tasks[i] = http.PostAsJsonAsync(writtenMemory.Span[i]);

    await Task.WhenAll(tasks); // wait for all requests
    
    for (var i = 0; i < writtenMemory.Length; i++) 
    {
        var result = taskBuffer[i].Result; // Assuming all exceptions are handled so .Result won't throw 
        // Do other stuff
    }

    for (var i = 0; i < writtenMemory.Length; i++) taskBuffer[i] = fromResult; // Set as completed
}

```

As you can see, now we don't allocate array each time, even if batch has only one element, we can still use ``Task.WhenAll`` as all other tasks in ``taskBuffer`` are completed. Note that we should also cleanup our ``taskBuffer`` after each batch. The best place for this is finally block of course, but for simplicity we will ignore this.

Congrats, you've invented a ``TaskBuffer<T>``.
Let's rewrite previous example with ``TaskBuffer<T>``:

``` csharp

// Pseudocode

var batchSize = 1_000;
var client = new HttpClient("someapi.com");
var taskBuffer = new TaskBuffer<T>(batchSize);

// Emulate a thread that reads batches and sends data to HttpClient
while (true) 
{
    using var rent = GetBatch(queue, batchSize); // GetBatch method from section (2), returns Rent<T>.

    for (var i = 0; i < writtenMemory.Length; i++) taskBuffer.Append(http.PostAsJsonAsync(writtenMemory.Span[i]));

    await Task.WhenAll(tasks); // wait for all requests, implicit conversion to Task[]
    
    var taskBufferMemory = taskBuffer.WrittenMemory;
    for (var i = 0; i < taskBufferMemory.Length; i++) 
    {
        var result = taskBufferMemory.Span[i].Result; // Assuming all exceptions are handled so .Result won't throw 
        // Do other stuff
    }

    taskBuffer.Clear();
}

```

As you can see the code is almost identical, ``TaskBuffer<T>`` also allocates an array for you and sets task as completed for each array entry. You can also use ``TaskBuffer`` for raw ``Task`` instances. Chunk of the code from source for better understanding: 

``` csharp

public sealed class TaskBuffer<T> : IBufferWriter<Task<T>>
{
    static readonly Task<T?> Completed = Task.FromResult(default(T));

    Task<T>[] _tasks;

    public TaskBuffer(int size)
    {
        _tasks = new Task<T>[size];
        for (var i = 0; i < _tasks.Length; i++) _tasks[i] = Completed;
    }

    public int Written { get; private set; }

    public bool IsFull => Written == _tasks.Length;
    public bool HasSome => Written > 0;


    // Methods from IBufferWriter
}

```

# 4. BufferWriter

``BufferWriter<T>`` is a simple implementation of ``IBufferWriter<T>`` that can grow by using arrays from ``ArrayPool<T>``. You can use this implementation everywhere where ``IBufferWriter<T>`` is requested.

For example, serializing json without allocating new array:

``` csharp

public readonly record ImportantData(decimal AmountPaid);

using var bufferWriter = new BufferWriter<byte>(1_024);

var utf8Writer = new Utf8JsonWriter(bufferWriter, new JsonWriterOptions()); // Writer is a class and can be pooled, check ObjectRent<T>

JsonSerializer.Serialize(utf8Writer, new ImportantData(10M));

var writtenSpan = bufferWriter.WrittenSpan;

// Do something with span - write to socket, etc...

```

``BufferWriter<T>`` is a simple wrapper around ``Rent<T>``:

``` csharp

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

    public bool IsFull => _rent.IsFull;
    public bool HasSome => _rent.HasSome;
    public int Available => _rent.Available;
    public int Written => _rent.WrittenSpan.Length;

    // Other stuff from IBufferWriter<T>
}

```

This class basically proxies most of the method calss to a ``Rent<T>``, which uses ``ArrayPool<T>``. Once buffer is full or the size is small, ``BufferWriter<T>`` will grow by copying arrays content:

```csharp

void Grow(int sizeHint)
{
    var growSize = Math.Max(sizeHint + _rent.Written, _rent.Written * 2);
    
    var oldRent = _rent;
    _rent = new Rent<T>(growSize);

    oldRent.WrittenSpan.CopyTo(_rent.GetSpan(oldRent.Written));
    oldRent.Dispose();
}

```

# 5. ArrayBufferWriter

``ArrayBufferWriter<T>`` also implements ``IBufferWriter<T>`` but it can't grow.
Once it's full it will throw an ``InvalidOperationException`` if you'll try to insert.

It is useful if you know that your data won't grow to certain size, for example if 1KiB is enough.

You can also use it for batching, for example, if you have a batch of requests that you can send to another API: 

``` csharp

// Pseudocode

public readonly record ImportantData(decimal AmountPaid);

// Method that sends data to remote API
public async Task<Result<Unit, Exception>> SendToApi(this HttpClient client, ImportantData data) 
{
    try
    {
        await client.PostAsJsonAsync(data);
        return new Result<Unit, Exception>(Unit.Shared);
    }
    catch (Exception ex) 
    {
        return new Result<Unit, Exception>(ex);
    }
}

// Assuming that batch size won't be larger that 1_000
public static class Batch 
{
    public static readonly int BatchSize = 1_000;
}


// Simple handler that send requests in batches (Ignore thread safety, assuming it's called from one thread)
public static class Handler 
{
    static readonly TaskBuffer<Task<Result<Unit, Exception>>> _taskBuffer = new (Batch.BatchSize);
    static readonly ArrayBufferWriter<ImportantData> _buffer = new (Batch.BatchSize);
    static readonly ArrayBufferWriter<ImportantData> _retryBuffer = new (Batch.BatchSize);

    public async Task<Result<Unit, Exception>> SendWithRetries(this HttpClient client, ImportantData[] batch, int retries) 
    {
        for (var i = 0; i < batch.Length; i++) _buffer.Append(batch[i]); // Copy everything to a buffer
        while (retries-- != 0) 
        {
            var dataToProcess = _buffer.WrittenSpan; // available data to process
            for (var i = 0; i < dataToProcess.Length; i++) _taskBuffer.Append(client.SendToApi(dataToProcess[i])); // Add requests to taskbuffer

            Task.WhenAll(_taskBuffer); // Wait for all tasks to be completed

            var completedTasks = _taskBuffer.WrittenSpan;
            for (var i = 0; i < completedTasks.Length; i++) 
            {
                var result = completedTasks[i].Result; // We can do this, our SendToApi method catches an exception and returns Result<T1, T2>
                if (!result.IsOk) _retryBuffer.Append(dataToProcess[i]); // If we got an error - append to retry buffer
                else {} // Else log successful execution
            } 

            _taskBuffer.Clear(); // Cleanup tasks
            _buffer.Clear(); // Cleanup buffer with ImportantData, we have already sent requests
            if (!_retryBuffer.HasSome) return new Result<Unit, Exception>(new Unit()); // Success, we have nothing to retry

            _retryBuffer.CopyTo(_buffer); // Copy content of retry buffer to buffer, we will retry once more
            _retryBuffer.Clear(); // Clean up retry buffer for next iteration

            await Task.Delay(TimeSpan.FromSeconds(2 << retry)); // Delay between requests on each retry (if needed)
        }

        // This error message and error result is Ok for test implementation.
        return new Result<Unit, Exception>(new InvalidOperationException($"Failed to send requests. Errors count: {_buffer.Written}));
    }
}

```

In this example we receive a batch of ``ImportantData`` and max number of retries, we will assume that all errors are transient, but in real implementation you'll probably check if it is so. Once we got this batch we can copy it to our ``_buffer`` which we can use on each retry, buffer will contain data that needs to be sent once more or initially. To send api requests we use ``TaskBuffer<T>`` which has preallocated array of ``Task`` instances. Then we use ``Task.WhenAll`` to wait for all requests to finish, pretty standard approach. Then in for loop each result is checked and if something went wrong we use ``Append`` to ``_retryBuffer`` so we can retry our requests. We also clear ``_taskBuffer`` and  ``_buffer`` on each retry using ``Clear`` so they are empty before each iteration. If ``_retryBuffer`` is empty (``!HasSome``) we can simply assume that everything went Okay and we can just return Ok result, if not - we can copy all data which we have to retry to our ``_retry`` buffer and try once more.

This code looks huge, but if it is called on a hot path, for example saving millions of records to Kafka or sending thousands requests you can save lots of allocations because everything is preallocated (of course new tasks are created for ``SendToApi`` method and ``ImportantData[]`` is also allocated (you can use ``Rent<ImportantData>``) + allocations for each ``ImportantData``), but at least you won't create huge arrays on retries, etc.


# 6. ObjectRent

``ObjectRent<T>`` is a simple ``readonly struct`` that allows you to return instances of ``class``es to ``ObjectPool<T>``. It can be useful if you don't want to allocate lots of object instances and want to reuse them. Let's check how this ``struct`` looks like:

``` csharp

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
        _pool!.Return(_value);

        _pool = null;
        _value = null;
    }
}

```

As you can see it's a ``struct`` which lazily rents instances of classes from ``ObjectPool<T>`` using ``Value`` property + it's ``IDisposable`` implementation which returns rented instance to pool. ``Action<T>`` return policy is used to do something with instances once they are returned, like setting all of fields to ``null`` or ``0``, etc.

You have noticed that it requres ``ObjectPool<T>`` to be provided, so it's cumbersome to initialize such instances, like in this example:

``` csharp

// Pseudocode

static readonly ObjectPool<T> _pool = new (); // Simple constructor for this example, in reality you can't initialize this class as shown here

// Simple class for example
public sealed class AppData 
{
    public string Text { get; set; }
    public int SomeValue { get; set; }
}

// possible method that uses such struct
public static void Example()
{
    using var rent = new ObjectRent<AppData>(_pool.Get(), data => 
    {
        data.Text = null;
        data.SomeValue = 0;
    });

    var appData = rent.Value;

    appData.Text = "Current data";
    appData.SomeValue = 1000;

    // Do something with instance of appData
}


```

In example above we used to shared instance of ``ObjectPool<T>`` to initialize ``ObjectRent<AppData>``. Notice that we also provide ``Action<T>`` to the constructor. This action is called on ``Dispose`` method to clean up data that we used in our instance, so state won't be corrupted on next rent :) It's up to you what to do here, you can ignore this parameter if you want. Notice that we should initialize our ``ObjectRent<T>`` instance by providing ``ObjectPool<T>`` instance each time, it's not that convenient.

To mitigate this you can use ``IPool<T>`` interface:

``` csharp

public interface IPool<T> where T : class
{
    ObjectRent<T> Get();
}

```

It's a simple interface that has only one method - ``Get`` which returns ``ObjectRent<T>`` instance. Let's check how you can write your own pool using a pool from this library - ``BufferWriterPool<T>`` - simple pool of ``BufferWriter<T>`` instances.


``` csharp

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

```

This is an implementation of ``BufferWriter<T>`` pool that you can use from this library. By default it will create ``BufferWriter<T>`` instances with staring capacity of ``DefaultCapacity`` (10), it also initializes a pool and uses special ``CapacityPolicy`` internally + you also get ``Shared`` instance like in ``ArrayPool<T>.Shared``. The most important part of this class is a ``Get`` implementation as it just returns new ``ObjectRent<BufferWriter<T>>`` instance that will ``Clear`` ``BufferWriter<T>`` when you'll call a ``Dispose`` method on a ``ObjectRent<T>``. Note that you should not ``Dispose`` a ``BufferWriter<T>`` instance because it will remove an array from ``BufferWriter<T>``, disposing ``ObjectRent`` is enough.

Another example of such usage is to reuse ``Context`` class from ``Polly`` for policies. Each time your policy is invoked you might initialize a new ``Context`` class which is a lot of allocations on hot paths, you can pool them instead.


``` csharp

// Pseudocode

public static class ContextKeys
{
    public static readonly string OperationName = nameof(OperationName);
    public static readonly string Data1 = nameof(Data1);
    public static readonly string Data2 = nameof(Data2);

    // and so on.
}

public sealed class ContextPool : IPool<Context> // Context here is from Polly library
{
    readonly ObjectPool<Context> _pool;

    public BufferWriterPool(string contextName) => _pool = new DefaultObjectPool<BufferWriter<T>>(new ContextNamePolicy(contextName));

    public ObjectRent<BufferWriter<T>> Get() => new (_pool, c => 
    {
        c[ContextKeys.OperationName] = null;
        c[ContextKeys.Data1] = null;
        c[ContextKeys.Data2] = null;
    });

    sealed class ContextNamePolicy : PooledObjectPolicy<Context>
    {
        readonly string _context;

        public ContextNamePolicy(int context) => _context = context;

        public override ContextNamePolicy Create() => new (_context);

        public override bool Return(Context obj) => true;
    }
}


public static Task Example(AsyncPolicy policy, ContextPool pool) 
{
    using var contextRent = pool.Get();
    var context = rent.Value;
    context[ContextKeys.OperationName] = "Exmaple";
    context[ContextKeys.Data1] = "Test";
    context[ContextKeys.Data2] = "Test";

    // Pseudocode:
    await _policy.ExecuteAsync(); // ExecuteAsync with context
}

```

As you can see here we've created a simple pool of ``Context`` objects, we also have a logic to cleanup data from context once it is returned to a pool. It's the only one of examples where you can use this approcah, it's up to you of course, note that this might be useful only on hot paths where allocations are really expensive.

# 7. Maps

Everybody use ``HashSet<T>`` which saved our lives many times, for example if you want to check that array contains only unique elements during validation. But sometimes it's an overkill, as you see - ``HashSet<T>`` is a class + it has complex logic inside, for simple use cases we can use a bit map instead. For example if you have an array of enum values, what you can do is to check if passed values are unique using ``BitMap`` which is a wrapper around ``ulong``:

``` csharp

public struct BitMap : IEquatable<BitMap>
{
    ulong _data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int bit) => _data |= 1UL << bit;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(int bit) => _data &= ~(1UL << bit);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Negate() => _data = ~_data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSet(int bit) => (_data & (1UL << bit)) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => _data = 0UL;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(BitMap other) => _data == other._data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? obj) => obj is BitMap other && Equals(other);

    public static explicit operator ulong(BitMap map) => map._data;
}

```

The API of ``HashSet<T>`` is almost the same, you can add, remove and check if element exists already. Let's see how you can use it in an example. Let's imagine that customer can only apply discount of some type only once for next purchase - you can combine 5% discount with 10% discount, but can't do 10% with 10% or 50% with 50%, possible solution is to recieve array of discounts that customer has used and add each discount type to hash set, if it already exists - return error, you can do the same with ``BitMap``:


``` csharp

// Pseudocode

public enum Discount : byte {
    FivePercent,
    TenPercent,
    TwentyPercent,
    ThirtyPercent,
    FortyPercent,
    FiftyPercent
}

public readonly record Request(Discount[] Discounts); // Discounts picked by a customer

public bool Validate(Request request) { // using bool here for validation, true - valid
    var bitMap = new BitMap();
    foreach (var discount in request.Discounts) 
    {
        if (bitMap.Contains((int)discount)) return false;
        bitMap.Add((int)discount);
    }

    return true;
}


```

Here in ``Validate`` method we use ``BitMap`` to check if particular discount was already applied.
We can do the same with ``HashMap<T>`` of course, but using ``BitMap`` is faster and it does not allocate anything.

Another useful map type which you might use is ``PartitionMap``. This map stores two ``uint``s values - ``All`` (which stores desired state) and current that you can set using ``Add``. ``PartitionMap`` is a ``BitMap`` which you can use to verify that particular combination is met:

``` csharp

public struct PartitionMap : IEquatable<PartitionMap>
{
    ulong _all;
    ulong _data;

    public PartitionMap(ulong all)
    {
        _data = 0;
        _all = all;
    }

    public PartitionMap(Span<int> all)
    {
        _all = 0;
        _data = 0;
        for (var i = 0; i < all.Length; i++) _all |= (1UL << all[i]);
    }

    public bool AllSet => _all == _data;

    // Same methods as bit map

}

```

As you can see here - ``PartitionMap`` stores desired state and also has ``AllSet`` property that you can use to check if it is met.
You can use it with any system that uses partitioning, for example Kafka - you can setup batching by size or when your consumer hits the end of all partitions using ``IsPartitionEOF``.


``` csharp

// Pseudocode

var consumer = new ConsumerBuilder<byte[], byte[]>(new ConsumerConfig { BootstrapServers = "host:port", EnablePartitionsEof = true }).Build();

consumer.Subscribe("topic-with-30-partitions");

var bufferWriter = new ArrayBufferWriter<byte[]>(1_000);

var map = new BitMap(Enumerable.Repeat(1, 30).ToArray()); // Set up partition map to have 30 first bits as 1 as desired state

// Consuming

while (true) {
    var message = consumer.Consume();

    // Adding eof to bitmap
    if (message.IsPartitionEOF) map.Add(message.Partition);
    else map.Remove(message.Partition);

    bufferWriter.Append(message.Value);

    if (!map.AllSet && !bufferWriter.IsFull) continue; // Batch is not full and we haven't reached end of all partitions - we can consume more


    // Do something with batch, etc


    // Cleanup your batch
    bufferWriter.Clear();
}

```

You can introduce simple yet effective batching using ``ArrayBufferWriter<T>`` and ``PartitionMap`` for the consumer.


# 8. Partitioners

If you want to introduce partitioning to your application you can use ``HashPartitioner`` or ``RandomPartitioner``, depending on your use case. ``RandomPartitioner`` uses ``Random`` class under the hood and decides a partition using ``Random`` call:

``` csharp

public static class RandomPartitioner
{
    static readonly Random Rand = new (Guid.NewGuid().GetHashCode());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetPartition(int partitions) => Rand.Next(0, partitions);
}

```

All you have to do is to call ``GetPartition`` with total number of partitions. Use it when it does not matter to which partition a particular key is bound.


``HashPartitioner`` uses ``GetHashCode`` method to chose a partition of your data. It is useful when your key should be bound to partition, for example like in Kafka where messages with concrete key will appear in one partition only.

``` csharp

public static class HashPartitioner
{
    public static int GetPartition<T>(T value, int partitions)
    {
        if (value is null || partitions <= 1) return 0;

        var mod = value.GetHashCode() % partitions;
        return mod < 0 ? mod + partitions : mod;
    }
}

```

``HashPartition`` also accepts a key to calculate partition.

When does such partitioning might be useful? For example if you use lock on some collection. What you can do instead is to create multiple instances of such collection in array and use partitioning on that array, this way you can decrease lock contention, check this example:

``` csharp

// Pseudocode

var partitions = new Queue<int>[4];
for (var i = 0; i < partitions.Length; i++) partitions[i] = new Queue<int>();

// Writer thread
Task.Run(() => {
    var num = 0;
    while (true) 
    {
        var partition = HashPartitioner.GetPartition(num, 4);
        lock (partitions[partition]) {
            partitions[partition].Enqueue(num++);
        }

        Thread.Sleep(100);
    }
});


// Readers
var readers = new Task[4];
for (var i = 0; i < 4; i++) readers[i] = Task.Run(partition => 
{
    var queue = partitions[partition];
    while (true) {
        lock (queue) 
        {
            if (queue.TryDequeue(out var value)) Console.WriteLine($"Value: {value}");
        }
        Thread.Sleep(250);
    }
}, i);

```

In this example we use 4 readers and one writer. Data is partitioned so you don't have to lock a single queue, you can lock a partition instead, readers do not interleave with each other.

# 9. ArrayStack

``ArrayStack<T>`` allows you to create a stack with underlying array on a stack :) note that ``T`` should be unmanaged. You can perform ``Pop`` and ``Push`` operations on an internal array.

``` csharp

public ref struct ArrayStack<T> where T : unmanaged
{
    int _index;
    readonly Span<T> _span;

    public ArrayStack(Span<T> span)
    {
        _index = 0;
        _span = span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T v) => _span[_index++] = v;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset() => _index = 0;

    public ReadOnlySpan<T> WrittenSpan => _index == 0 ? ReadOnlySpan<T>.Empty : _span.Slice(0, _index);
}

```

Note that this is a ``ref struct`` so it can only be created on a stack. Note that it uses ``Span<T>`` as a storage of data so you can do something like:

``` csharp

var arrayStack = new ArrayStack<int>(stackalloc int[64]);

// Push Pop etc

```

In this example data is located on a stack so you can manipulate it as you wish without allocating on a heap. You can use ``ArrayStack<T>`` for parsing logic. Let's parse dotnet diagnostics query with format ``{counter-provider}[{counter-1},{counter-2},..],...,{counter-provider}[{counter-1},...]`` by writing a parser for such query: ``System.Runtime[assembly-count],Microsoft.AspNetCore.Hosting``:

``` csharp

// Pseudocode

public enum Stage : byte { Counter = 1, Provider = 2 }; // Parsing stage - provider (System.Runtime) or counter (assembly-count)

static Result<(HashSet<string> Providers, HashSet<string> Counters)> Parse(ReadOnlySpan<char> query)
{
    var stage = Stage.Provider;
    var filter = (Providers: new HashSet<string>(), Counters: new HashSet<string>());
    var stack = new ArrayStack<char>(stackalloc char[64]); // 64 is more than enough

    try
    {
        var index = 0;
        while (index++ < query.Length)
        {
            switch (query[index])
            {
                case '[':
                case ',' when stage == Stage.Provider: filter.Providers.Add(stack.WrittenSpan.ToString()); stack.Reset(); break;

                case ']':
                case ',' when stage == Stage.Counter: filter.Counters.Add(stack.WrittenSpan.ToString()); stack.Reset(); break;

                default: stack.Push(query[index]); break;
            }

            stage = query[index] switch
            {
                '[' => Stage.Counter,
                ']' => Stage.Provider,
                _ => stage
            };
        }

        // Latest value should be a provider
        filter.Providers.Add(stack.Written.ToString());

        return new Result<(HashSet<string> Providers, HashSet<string> Counters)>(filter);
    }
    catch (Exception ex)
    {
        return new Result<(HashSet<string> Providers, HashSet<string> Counters)>(ex);
    }
}

```

We use ``Result<T>`` and wrap an ``Exception`` if something goes wrong, note that ``ArrayStack<T>`` also exposes a property ``WrittenSpan`` so you can get part of array that is written to it using ``Push`` or ``Pop``, we use it to build a string, for example: ``System.Runtime`` or ``gc-count``, we also use ``Stage`` to understand what should be parsed - counter or provider.

# 10. Arc

`Arc<T>` is a simple atomic reference counting implementation that allows you to drop an instance once all of threads are done using it. `Arc` also allows you to specify an `Action<T>` which will be called once it's internal counter is dropped to 0 so `Arc<T>` is not used by anyone and internal resource can be dropped, `Arc` also handles `IDisposable` cases and calls a `Dispose` method on drop:

``` csharp

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
    

    // other stuff
}
```

As you can see you call a `Rent` method to rent a value from `Arc`, this action increments `Arc`'s internal counter. Once you're done with using a value - you can call `Dispose` on returned rent, this is when internal counter is dropped and potentially your value's `Dispose` is called.

``` csharp

// Pseudocode:

var arc = new Arc<SharedResource>();

using var rent = arc.Rent(); // Counter got incremented
rent.Value.DoStuff();

// This is where rent.Dispose() is called and internal counter is decremented

```

A potential real-world example for `Arc` is issue with C# Redis client implementation when you use Kubernetes that does not guarantees stable IPs for Redis nodes so when instance is restarted a new IP is assigned to Pod and `StackExchange.Redis` implementation can't reconnect. What you can do instead is to track connection issues store `Arc` and recreate an `Arc` once connection issue occured so threads that use old `Arc` will dispose internal Redis client and threads will start use new Redis client's instance because you recreated an `Arc`:

``` csharp

// Pseudocode

// NOTE: This code doesn't handle exceptions you should retry in action block or send command (Unit) once more after some time
// Wrapper around Redis ConnectionMultiplexer
public sealed class RedisClient : IDisposable
{
    ArcRent<IRedisConnection> _redisRent; // Store a rent so counter is 1 at minimum

    volatile Arc<IRedisConnection> _arc; // Arc
    readonly ActionBlock<Unit> _refresher; // Or you can use Channels

    public RedisClient(string connectionString)
    {
        _refresher = new ActionBlock<IRedisConnection>(c => 
        {
            var oldRent = _redisRent;
            if (oldRent.Value != c) return; // Duplicate message or outdated, current rent and passed client do not match

            var client = ConnectionMultiplexer.Connect(connectionString); // Create connection to redis
            client.ConnectionFailed += (s, e) => _refresher.Post(client); // once issue occurs we send a message to action block
            
            _arc = new Arc<IRedisConnection>(client);
            _redisRent = _arc.Rent();

            oldRent.Dispose(); // Drop an old rent. This will decrement a counter. If it still > 0 other thread will dispose redis client
        });

        var client = ConnectionMultiplexer.Connect(connectionString); // Create connection to redis
        client.ConnectionFailed += (s, e) => _refresher.Post(client); // once issue occurs we send a message to action block

        _arc = new Arc<IRedisConnection>(client);
        _redisRent = _arc.Rent();
    }

    public ArcRent<IRedisConnection> Rent() => _arc.Rent();

    public void Dispose() 
    {
        _refresher.Complete();
        _refresher.Completion.Wait();
        _redisRent.Dispose();
    }
}

// Main

var client = new RedisClient("connection");

// Somewhere in controller action/method/etc

using var redisRent = provider.Rent();
// Do something with redis client
redisRent.Value.Somemethod();

// Rent will be disposed and counter will be decremented

```

# 11. ValueRent

`ObjectRent<T>` is strongly dependent on `ObjectPool<T>` but sometimes you don't use such pools under the hood but still want to "rent" value from some source and then return it back. Since `ObjectRent<T>` does not meet such requirement - `ValueRent<T, TContext>` and `ValueRent<T>` were created. The difference between `ValueRent<T>` and `ValueRent<T, TContext>` is that `ValueRent<T>` does not have generic `context` it is of type `object`. Context allows you to wrap some context that you want to use when you return an instance, you can ignore it altogether, this is needed in order to not allocate a new object (explained later).

Let's check examples:

``` csharp

// Pseudocode

// Creating an object pool
var pool = new ObjectPool<T>(...);

// Create a rent getting value from a pool and providing a pool as a context
using var rent = new ValueRent<T>(pool.Get(), pool, (rentedValue, context) => {
    // Once rent is disposed we want to return value to a pool
    var pool = (ObjectPool<T>)context;
    pool.Return(rentedValue);
});


// Do something with rentedValue
ImportantMethod(rent.Value);

// NOTE: we could have do someting like this:
using var rent = new ValueRent<T>(pool.Get(), (rentedValue, _) => {
    // Once rent is disposed we want to return value to a pool
    pool.Return(rentedValue);
});


// We did not pass pool as a context above. This is perfectly fine, but we allocated a closure
// in an Action, context parameter is provided for such cases where you don't want to allocate
// additional objects

```

As you can see we completely emulated the behavior that `ObjectRent<T>` provides using `ValueRent<T>`, we could have used also `ValueRent<T, ObjectPool<T>>` instead, it depends on your preference.