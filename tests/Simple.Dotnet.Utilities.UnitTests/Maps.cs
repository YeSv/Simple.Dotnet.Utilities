namespace Simple.Dotnet.Utilities.UnitTests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Utilities.Maps;
    using Xunit;

    public sealed class Maps
    {
        [Fact]
        public void BitMap_Should_Cast_To_Ulong_Correctly()
        {
            for (var i = 0; i < 64; i++)
            {
                var map = new BitMap();
                map.Add(i);

                var value = (ulong)map;
                value.Should().Be((ulong)Math.Pow(2, i), $"Value should be the same on iteration: {i}");
            }
        }

        [Fact]
        public void BitMap_Should_Cast_To_Ulong_Correctly_Accumulated()
        {
            var map = new BitMap();
            var accumulator = 0UL;

            for (var i = 0; i < 64; i++)
            {
                map.Add(i);
                accumulator += (ulong)Math.Pow(2, i);

                ((ulong)map).Should().Be(accumulator, $"Value should be the same on iteration: {i}");
            }
        }

        [Fact]
        public void BitMap_Should_Remove_Correct_Bit()
        {
            var bitMap = new BitMap();
            for (var i = 0; i < 64; i++) bitMap.Add(i);
            
            var copy = bitMap;

            copy.Equals(bitMap).Should().BeTrue();

            for (var i = 0; i < 64; i++)
            {
                bitMap.Remove(i);
                bitMap.Equals(copy).Should().BeFalse();
                bitMap.Add(i);
                bitMap.Equals(copy).Should().BeTrue();
            }
        }

        [Fact]
        public void BitMap_Should_Correctly_Check_Set_Bit()
        {
            var bitMap = new BitMap();
            for (var i = 0; i < 64; i++)
            {
                bitMap.Add(i);
                bitMap.IsSet(i).Should().BeTrue();
            }

            for (var i = 0; i < 64; i++)
            {
                bitMap.Remove(i);
                bitMap.IsSet(i).Should().BeFalse();
            }

            bitMap.Equals(new BitMap()).Should().BeTrue();
        }

        [Fact]
        public void BitMap_Should_Clear_All_Bits()
        {
            var bitMap = new BitMap();

            for (var i = 0; i < 64; i++)
            {
                bitMap.Add(i);
                bitMap.Clear();
                bitMap.IsSet(i).Should().BeFalse();
            }
        }

        [Fact]
        public void BitMap_Should_Correctly_Negate()
        {
            var full = new BitMap();
            full.Negate();

            var empty = new BitMap();
            for (var i = 0; i < 64; i++) empty.Add(i);

            empty.Equals(full).Should().BeTrue();

            full.Negate();
            full.Equals(new BitMap()).Should().BeTrue();

            var bitMap = new BitMap();
            bitMap.Negate();
            bitMap.Negate();

            bitMap.Equals(new BitMap()).Should().BeTrue();
        }




        [Fact]
        public void PartitionMap_Should_Cast_To_Ulong_Correctly()
        {
            for (var i = 0; i < 64; i++)
            {
                var map = new PartitionMap();
                map.Add(i);

                var value = (ulong)map;
                value.Should().Be((ulong)Math.Pow(2, i), $"Value should be the same on iteration: {i}");
            }
        }

        [Fact]
        public void PartitionMap_Should_Cast_To_Ulong_Correctly_Accumulated()
        {
            var map = new PartitionMap();
            var accumulator = 0UL;

            for (var i = 0; i < 64; i++)
            {
                map.Add(i);
                accumulator += (ulong)Math.Pow(2, i);

                ((ulong)map).Should().Be(accumulator, $"Value should be the same on iteration: {i}");
            }
        }

        [Fact]
        public void PartitionMap_Should_Remove_Correct_Bit()
        {
            var bitMap = new PartitionMap();
            for (var i = 0; i < 64; i++) bitMap.Add(i);

            var copy = bitMap;

            copy.Equals(bitMap).Should().BeTrue();

            for (var i = 0; i < 64; i++)
            {
                bitMap.Remove(i);
                bitMap.Equals(copy).Should().BeFalse();
                bitMap.Add(i);
                bitMap.Equals(copy).Should().BeTrue();
            }
        }

        [Fact]
        public void PartitionMap_Should_Correctly_Check_Set_Bit()
        {
            var bitMap = new PartitionMap();
            for (var i = 0; i < 64; i++)
            {
                bitMap.Add(i);
                bitMap.IsSet(i).Should().BeTrue();
            }

            for (var i = 0; i < 64; i++)
            {
                bitMap.Remove(i);
                bitMap.IsSet(i).Should().BeFalse();
            }

            bitMap.Equals(new PartitionMap()).Should().BeTrue();
        }

        [Fact]
        public void PartitionMap_Should_Clear_All_Bits()
        {
            var bitMap = new PartitionMap();

            for (var i = 0; i < 64; i++)
            {
                bitMap.Add(i);
                bitMap.Clear();
                bitMap.IsSet(i).Should().BeFalse();
            }
        }

        [Fact]
        public void PartitionMap_Should_Correctly_Negate()
        {
            var full = new PartitionMap();
            full.Negate();

            var empty = new PartitionMap();
            for (var i = 0; i < 64; i++) empty.Add(i);

            empty.Equals(full).Should().BeTrue();

            full.Negate();
            full.Equals(new PartitionMap()).Should().BeTrue();

            var bitMap = new PartitionMap();
            bitMap.Negate();
            bitMap.Negate();

            bitMap.Equals(new PartitionMap()).Should().BeTrue();
        }

        [Fact]
        public void PartitionMap_Should_Check_AllSet_Array()
        {
            var bitMap = new PartitionMap();
            for (var i = 0; i < 64; i++)
            {
                bitMap = new PartitionMap(Enumerable.Repeat(1, i + 1).ToArray());
                for (var j = 0; j <= i; j++) bitMap.Add(j);

                bitMap.AllSet.Should().BeTrue();

                bitMap.Negate();
                bitMap.AllSet.Should().BeFalse();
            }
        }
    }
}
