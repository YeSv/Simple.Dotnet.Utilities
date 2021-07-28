namespace Simple.Dotnet.Utilities.UnitTests
{
    using Buffers;
    using FluentAssertions;
    using Utilities.Partitioners;
    using Xunit;

    public sealed class Partitioners
    {
        [Fact]
        public void HashPartitioner_Should_Return_0_Partition_On_Null()
        {
            HashPartitioner.GetPartition<object>(null, 10).Should().Be(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void HashPartitioner_Should_Return_0_Partition_On_Zero_One_Negative(int partitions)
        {
            HashPartitioner.GetPartition(1, partitions).Should().Be(0);
        }

        [Fact]
        public void HashPartitioner_Should_Return_Partition_In_Range()
        {
            for (var i = 0; i < 100; i++) HashPartitioner.GetPartition(i + 1, 10).Should().BeLessThan(10).And.BeGreaterOrEqualTo(0);
        }

        [Fact]
        public void HashPartitioner_Should_Return_Partition_In_Range_Classes()
        {
            for (var i = 0; i < 100; i++) HashPartitioner.GetPartition(new BufferWriter<int>(i + 1), 10).Should().BeLessThan(10).And.BeGreaterOrEqualTo(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void RandomPartitioner_Should_Return_0_Partition_On_Zero_One_Negative(int partitions)
        {
            RandomPartitioner.GetPartition(partitions).Should().Be(0);
        }


        [Fact]
        public void RandomPartitioner_Should_Return_Partition_In_Range()
        {
            for (var i = 0; i < 100; i++) RandomPartitioner.GetPartition(10).Should().BeLessThan(10).And.BeGreaterOrEqualTo(0);
        }
    }
}
