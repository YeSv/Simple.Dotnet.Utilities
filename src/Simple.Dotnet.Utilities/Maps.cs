namespace Simple.Dotnet.Utilities.Maps
{
    using System;
    using System.Runtime.CompilerServices;
    
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
            for (var i = 0; i < all.Length; i++) _all |= ((ulong)all[i] << i);
        }

        public bool AllSet => _all == _data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(int bit) => _data |= (1UL << bit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(int bit) => _data &= ~(1UL << bit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Negate() => _data = ~_data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(int bit) => (_data & (1UL << bit)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => _data = 0UL;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(PartitionMap other) => _all == other._all && _data == other._data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is PartitionMap other && Equals(other);

        public static explicit operator ulong(PartitionMap map) => map._data;
    }
}
