namespace Simple.Dotnet.Utilities.Results
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

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
    
    public readonly struct UniResult<T> : IEquatable<UniResult<T>> where T : class
    {
        public readonly object? Data;

        public UniResult(T data) => Data = data;
        public UniResult(object? error) => Data = error;

        public bool IsOk => Data is T or null;

        public override int GetHashCode() => Data?.GetHashCode() ?? 0;

        public bool Equals(UniResult<T> other)
        {
            if (Data == null || other.Data == null) return Data == other.Data;
            if (Data is T && other.Data is T && Data is IEquatable<T> equatable) return equatable.Equals((IEquatable<T>)other.Data);
            return Data.Equals(other.Data);
        }

        public override bool Equals(object obj) => obj is UniResult<T> other && Equals(other);

        public override string ToString() => Data as string ?? Data?.ToString() ?? "Result with null data";

        public static explicit operator string(in UniResult<T> result) => result.ToString();
        
        public static explicit operator Exception(in UniResult<T> result) => result.Data is Exception ex ? ex : throw new InvalidOperationException("Failed to cast inner data to exception");

        public static explicit operator T(in UniResult<T> result) => (T) result.Data!;
    }

    public static class UniResult
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniResult<T> Ok<T>(T data) where T : class => new (data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniResult<T> Error<T>(object? error) where T : class => new (error);
    }

    public readonly struct Result<T> : IEquatable<Result<T>>
    {
        public readonly T? Data;
        public readonly object? Error;

        public bool IsOk => Error == null;

        public Result(T data)
        {
            Data = data;
            Error = null;
        }

        public Result(object error)
        {
            Error = error;
            Data = default;
        }

        public bool Equals(Result<T> other)
        {
            if (IsOk != other.IsOk) return false;

            if (Error != null || other.Error != null)
            {
                if (Error == null || other.Error == null) return Error == other.Error;
                return Error.Equals(other.Error);
            }
            
            if (Data is null && other.Data is null) return true;
            if (Data is null && other.Data is not null || Data is not null && other.Data is null) return false;
            if (Data is IEquatable<T> d1) return d1.Equals((IEquatable<T>)other.Data!);
            
            return Data!.Equals(other.Data!);
        }

        public override bool Equals(object? obj) => obj is Result<T> other && Equals(other);

        public override int GetHashCode() => Error?.GetHashCode() ?? Data?.GetHashCode() ?? 0;

        public override string ToString() => Error?.ToString() ?? Data?.ToString() ?? "Result with null data";

        public static explicit operator string(in Result<T> result) => result.ToString();

        public static explicit operator Exception(in Result<T> result) => result.Error is Exception ex ? ex : throw new InvalidOperationException("Failed to cast inner data to exception");

        public static explicit operator T(in Result<T> result) => result.IsOk ? result.Data! : throw new InvalidOperationException("Result does not contain ok data");
    }

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

        public bool Equals(Result<TOk, TError> other)
        {
            if (IsOk != other.IsOk) return false;
            if (!IsOk)
            {
                if (Error is IEquatable<TError> e) return e.Equals(other.Error!);
                if (Error is null) return other.Error is null;
                return Error!.Equals(other.Error!);
            }

            if (Ok is null) return other.Ok is null;
            if (other.Ok is null) return Ok is null;
            
            if (Ok is IEquatable<TOk> ok) return ok.Equals(other.Ok!);
            return Ok!.Equals(other.Ok!);
        }

        public override bool Equals(object? obj) => obj is Result<TOk, TError> other && Equals(other);

        public override int GetHashCode() => IsOk ? Ok?.GetHashCode() ?? 0 : Error?.GetHashCode() ?? 0;

        public override string ToString() => Ok?.ToString() ?? Error?.ToString() ?? "Result with null data";


        public static explicit operator string(in Result<TOk, TError> result) => result.IsOk switch
        {
            true when result.Ok is string okStr => okStr,
            false when result.Error is string errorStr => errorStr,
            _ => result.ToString()
        };

        public static explicit operator TOk(in Result<TOk, TError> result) => result.IsOk ? result.Ok! : throw new InvalidOperationException("Result does not contain ok data");
        public static explicit operator TError(in Result<TOk, TError> result) => !result.IsOk ? result.Error! : throw new InvalidOperationException("Result does not contain error data");
    }

    public static class Result
    {
        public static readonly Result<Unit> UnitResult = new(new Unit());

        public static readonly Task<Result<Unit>> UnitTask = Task.FromResult(UnitResult);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Ok<T>(T data)  => new(data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<T> Error<T>(object error) => new(error);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TOk, TError> Ok<TOk, TError>(TOk data) => new(data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TOk, TError> Error<TOk, TError>(TError error) => new(error);
    }

    public static class ResultsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? Unwrap<T>(in this UniResult<T> result) where T : class => (T) result.Data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TOk, TError> To<TOk, TError>(in this UniResult<TOk> result) where TOk : class where TError : class => result.Data switch
        {
            TOk ok => new(ok),
            TError error => new(error),
            null => new((TOk?)null),
            _ => throw new InvalidOperationException("Result contains not expected type, not possible to cast")
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TError Error<TOk, TError>(in this UniResult<TOk> result) where TOk : class where TError: class => result.IsOk ? throw new InvalidOperationException("This is not an error result. Check IsOk.") : (TError) result.Data!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TError Error<TOk, TError>(in this Result<TOk> result) => result.IsOk ? throw new InvalidOperationException("This is not an error result. Check IsOk.") : (TError)result.Error!;
    }
}
