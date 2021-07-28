namespace Simple.Dotnet.Utilities.UnitTests
{
    using System;
    using Buffers;
    using FluentAssertions;
    using Xunit;

    public sealed class Rent
    {
        [Fact]
        public void Rent_Should_Not_Throw_On_Multiple_Dispose()
        {
            var rent = new Rent<int>(10);
            rent.Dispose();
            rent.Dispose();
        }

        [Fact]
        public void Rent_Should_Not_Throw_On_Multiple_Dispose_Zero_Length()
        {
            var rent = new Rent<int>();
            rent.Dispose();
            rent.Dispose();
        }

        [Fact]
        public void Rent_Should_Be_Initialized_As_Empty()
        {
            using var rent = new Rent<int>(0);
            rent.Length.Should().Be(0);
            rent.Written.Should().Be(0);
            rent.HasSome.Should().BeFalse();
            rent.Available.Should().Be(0);
            rent.IsFull.Should().BeTrue();
            rent.WrittenSpan.Length.Should().Be(0);
            rent.WrittenMemory.Length.Should().Be(0);
            rent.WrittenSegment.Array.Length.Should().Be(0);
        }

        [Fact]
        public void Rent_Should_Be_Initialized_As_Empty_Negative()
        {
            using var rent = new Rent<int>(-1);
            rent.Length.Should().Be(0);
            rent.Written.Should().Be(0);
            rent.HasSome.Should().BeFalse();
            rent.Available.Should().Be(0);
            rent.IsFull.Should().BeTrue();
            rent.WrittenSpan.Length.Should().Be(0);
            rent.WrittenMemory.Length.Should().Be(0);
            rent.WrittenSegment.Array.Length.Should().Be(0);
        }

        [Fact]
        public void Rent_Written_Should_Be_Empty_If_Not_Used()
        {
            using var rent = new Rent<int>(100);

            rent.Length.Should().Be(100);
            rent.Written.Should().Be(0);
            rent.HasSome.Should().BeFalse();
            rent.Available.Should().Be(100);
            rent.IsFull.Should().BeFalse();
            rent.WrittenSpan.Length.Should().Be(0);
            rent.WrittenMemory.Length.Should().Be(0);
            rent.WrittenSegment.Count.Should().Be(0);
        }

        [Fact]
        public void Rent_Advance_Should_Change_Values_Of_Properties()
        {
            using var rent = new Rent<int>(100);

            for (var i = 0; i < rent.Length; i++)
            {
                rent.Advance(1);
                rent.HasSome.Should().BeTrue();
                rent.Written.Should().Be(i + 1);
                rent.Available.Should().Be(rent.Length - (i + 1));
                rent.WrittenSpan.Length.Should().Be(i + 1);
                rent.WrittenMemory.Length.Should().Be(i + 1);
                rent.WrittenSegment.Count.Should().Be(i + 1);
            }

            rent.IsFull.Should().BeTrue();
        }

        [Fact]
        public void Rent_Advance_Should_Throw_On_Overflow()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var rent = new Rent<int>(100);
                for (var i = 0; i < 200; i++) rent.Advance(1);
            });
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(100)]
        public void Rent_Can_Advance(int step)
        {
            using var rent = new Rent<int>(10 * step);
            for (var i = 0; i < 10; i++) rent.Advance(step);

            rent.IsFull.Should().BeTrue();
        }

        [Fact]
        public void Rent_Cant_Advance_On_Negative_Value()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var rent = new Rent<int>(10);
                rent.Advance(-1);
            });
        }

        [Fact]
        public void Rent_Can_Advance_On_Zero()
        {
            using var rent = new Rent<int>(10);
            rent.Advance(0);
            rent.Written.Should().Be(0);
            rent.HasSome.Should().BeFalse();
        }

        [Fact]
        public void Rent_Append_Should_Update_Properties()
        {
            using var rent = new Rent<int>(10);

            for (var i = 0; i < rent.Length; i++)
            {
                rent.Append(i);
                rent.Written.Should().Be(i + 1);
                rent.HasSome.Should().BeTrue();
                rent.Available.Should().Be(rent.Length - (i + 1));
                rent.WrittenSpan.Length.Should().Be(i + 1);
                rent.WrittenMemory.Length.Should().Be(i + 1);
                rent.WrittenSegment.Count.Should().Be(i + 1);
                rent.GetSpan().Length.Should().Be(rent.Length - (i + 1));
                rent.GetMemory().Span.Length.Should().Be(rent.Length - (i + 1));
            }

            rent.IsFull.Should().BeTrue();
            rent.GetSpan().Length.Should().Be(0);
            rent.GetMemory().Span.Length.Should().Be(0);

            var writtenSpan = rent.WrittenSpan;
            for (var i = 0; i < writtenSpan.Length; i++) writtenSpan[i].Should().Be(i);

            var writtenMemory = rent.WrittenMemory;
            for (var i = 0; i < writtenMemory.Length; i++) writtenMemory.Span[i].Should().Be(i);

            var writtenSegment = rent.WrittenSegment;
            for (var i = 0; i < writtenSegment.Count; i++) writtenSegment[i].Should().Be(i);
        }

        [Fact]
        public void Rent_Append_Should_Throw_On_Overflow()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var rent = new Rent<int>(10);
                for (var i = 0; i < rent.Length * 2; i++) rent.Append(i);
            });
        }

        [Fact]
        public void Rent_GetMemory_Should_Throw_If_Negative()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var rent = new Rent<int>(10);
                rent.GetMemory(-1);
            });
        }

        [Fact]
        public void Rent_GetMemory_Should_Throw_If_Too_Large()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var rent = new Rent<int>(10);
                rent.GetMemory(rent.Length + 1);
            });
        }

        [Fact]
        public void Rent_GetMemory_Should_Return_Whole_Length()
        {
            using var rent = new Rent<int>(10);
            var memory = rent.GetMemory(rent.Length);
            memory.Length.Should().Be(rent.Length);
        }

        [Fact]
        public void Rent_GetSpan_Should_Throw_If_Negative()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var rent = new Rent<int>(10);
                rent.GetSpan(-1);
            });
        }

        [Fact]
        public void Rent_GetSpan_Should_Throw_If_Too_Large()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var rent = new Rent<int>(10);
                rent.GetSpan(rent.Length + 1);
            });
        }

        [Fact]
        public void Rent_GetSpan_Should_Return_Whole_Length()
        {
            using var rent = new Rent<int>(10);
            var span = rent.GetSpan(rent.Length);
            span.Length.Should().Be(rent.Length);
        }

        [Fact]
        public void Rent_GetSpan_Should_Shrink()
        {
            using var rent = new Rent<int>(10);
            for (var i = 0; i < rent.Length; i++)
            {
                rent.Advance(1);
                rent.GetSpan().Length.Should().Be(rent.Length - (i + 1));
            }

            rent.GetSpan().Length.Should().Be(0);
        }

        [Fact]
        public void Rent_GetMemory_Should_Shrink()
        {
            using var rent = new Rent<int>(10);
            for (var i = 0; i < rent.Length; i++)
            {
                rent.Advance(1);
                rent.GetMemory().Length.Should().Be(rent.Length - (i + 1));
            }

            rent.GetMemory().Length.Should().Be(0);
        }

        [Fact]
        public void Rent_Clear_Should_Reset_Rent()
        {
            using var rent = new Rent<int>(10);
            for (var i = 0; i < rent.Length; i++) rent.Advance(1);

            rent.Available.Should().Be(0);
            rent.IsFull.Should().BeTrue();

            rent.Clear();

            rent.Available.Should().Be(rent.Length);
            rent.IsFull.Should().BeFalse();
            rent.Written.Should().Be(0);
            rent.WrittenSpan.Length.Should().Be(0);
            rent.WrittenMemory.Length.Should().Be(0);
            rent.WrittenSegment.Count.Should().Be(0);
            rent.GetSpan().Length.Should().Be(rent.Length);
            rent.GetMemory().Length.Should().Be(rent.Length);


            for (var i = 0; i < rent.Length; i++) rent.Append(i);
            for (var i = 0; i < rent.Length; i++) rent.WrittenSpan[i].Should().Be(i);
        }

        [Fact]
        public void Rent_AsEnumerable_Should_Enumerate_On_Empty()
        {
            using var rent = new Rent<int>();
            foreach (var v in rent.AsEnumerable()) v.Should().Be(0); // Won't be called
        }

        [Fact]
        public void Rent_AsEnumerable_Should_Enumerate()
        {
            using var rent = new Rent<int>(10);
            
            var span = rent.GetSpan(10);
            for (var i = 0; i < span.Length; i++) span[i] = i;

            rent.Advance(10);

            var last = -1;
            foreach (var v in rent.AsEnumerable()) v.Should().Be(++last);
        }
    }
}
