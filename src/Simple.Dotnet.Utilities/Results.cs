namespace Simple.Dotnet.Utilities.Results
{
    using System;
    using System.Runtime.CompilerServices;

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
    
    public readonly struct UniResult<TOk, TError> : IEquatable<UniResult<TOk, TError>> where TOk : class where TError : class
    {
        public readonly object? Data;

        public UniResult(TOk? data) => Data = data;
        public UniResult(TError error) => Data = error;

        public bool IsOk => Data is TOk or null;
        public TOk? Ok => IsOk ? Data as TOk ?? null : throw new InvalidOperationException($"Result does not contain data of type {typeof(TOk)}");
        public TError? Error => !IsOk ? Data as TError ?? null : throw new InvalidOperationException($"Result does not contain data of type {typeof(TError)}");

        public override int GetHashCode() => Data?.GetHashCode() ?? 0;

        public bool Equals(UniResult<TOk, TError> other)
        {
            if (Data == null || other.Data == null) return Data == other.Data;
            return Data switch
            {
                TOk and IEquatable<TOk> okEquatable => okEquatable.Equals((IEquatable<TOk>) other.Data),
                TError and IEquatable<TError> errEquatable => errEquatable.Equals((IEquatable<TError>) other.Data),
                _ => Data.Equals(other.Data)
            };
        }

        public static void Deconstruct(in UniResult<TOk, TError> result, out TOk? ok, out TError? error)
        {
            ok = default; error = default;
            if (result.IsOk) ok = result.Ok;
            else error = result.Error;
        }

        public override bool Equals(object obj) => obj is UniResult<TOk, TError> other && Equals(other);

        public override string ToString() => Data as string ?? Data?.ToString() ?? "Result with null data";

        public static explicit operator TOk?(in UniResult<TOk, TError> result) => result.IsOk ? (TOk?)result.Data : throw new InvalidOperationException($"Result does not contain data of type: {typeof(TOk)}");

        public static explicit operator TError?(in UniResult<TOk, TError> result) => result.IsOk ? (TError?)result.Data : throw new InvalidOperationException($"Result does not contain data of type: {typeof(TError)}");
    }

    public static class UniResult
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniResult<TOk, TError> Ok<TOk, TError>(TOk data) where TOk : class where TError : class => new (data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UniResult<TOk, TError> Error<TOk, TError>(TError error) where TOk : class where TError : class => new (error);
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


        public static void Deconstruct(in Result<TOk, TError> result, out TOk? ok, out TError? error)
        {
            ok = default; error = default;
            if (result.IsOk) ok = result.Ok;
            else error = result.Error;
        }

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TOk, TError> Ok<TOk, TError>(TOk data) => new(data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TOk, TError> Error<TOk, TError>(TError error) => new(error);
    }

    public static class ResultsExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Result<TOk, TError> AsResult<TOk, TError>(in this UniResult<TOk, TError> result) where TOk : class where TError : class => result.Data switch
        {
            TOk ok => new(ok),
            TError error => new(error),
            null => new((TOk?)null),
            _ => throw new InvalidOperationException("Result contains not expected type, not possible to cast")
        };
    }
}
