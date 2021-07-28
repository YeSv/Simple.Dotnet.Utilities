namespace Simple.Dotnet.Utilities.Partitioners
{
    using System;
    using System.Runtime.CompilerServices;

    public static class HashPartitioner
    {
        public static int GetPartition<T>(T value, int partitions)
        {
            if (value is null || partitions <= 1) return 0;

            var mod = value.GetHashCode() % partitions;
            return mod < 0 ? mod + partitions : mod;
        }
    }

    public static class RandomPartitioner
    {
        static readonly Random Rand = new (Guid.NewGuid().GetHashCode());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPartition(int partitions) => partitions <= 1 ? 0 : Rand.Next(0, partitions);
    }
}
