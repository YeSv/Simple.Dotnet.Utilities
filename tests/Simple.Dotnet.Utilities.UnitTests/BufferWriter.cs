namespace Simple.Dotnet.Utilities.UnitTests
{
    using System;
    using Buffers;
    using FluentAssertions;
    using Xunit;

    public sealed class BufferWriter
    {
        [Fact]
        public void Rent_Should_Not_Throw_On_Multiple_Dispose()
        {
            using var writer = new BufferWriter<int>(10);
            writer.Dispose();
            writer.Dispose();
        }

        [Fact]
        public void Rent_Should_Not_Throw_On_Multiple_Dispose_Zero_Length()
        {
            using var writer = new BufferWriter<int>(0);
            writer.Dispose();
            writer.Dispose();
        }

        [Fact]
        public void BufferWriter_Should_Be_Initialized_As_Empty()
        {
            using var writer = new BufferWriter<int>(0);
            writer.Written.Should().Be(0);
            writer.HasSome.Should().BeFalse();
            writer.WrittenSpan.Length.Should().Be(0);
            writer.WrittenMemory.Length.Should().Be(0);
            writer.WrittenSegment.Array.Length.Should().Be(0);
        }

        [Fact]
        public void BufferWriter_Should_Be_Initialized_As_Empty_Negative()
        {
            using var writer = new BufferWriter<int>(-1);
            writer.Written.Should().Be(0);
            writer.HasSome.Should().BeFalse();
            writer.WrittenSpan.Length.Should().Be(0);
            writer.WrittenMemory.Length.Should().Be(0);
            writer.WrittenSegment.Array.Length.Should().Be(0);
        }

        [Fact]
        public void BufferWriter_Written_Should_Be_Empty_If_Not_Used()
        {
            using var writer = new BufferWriter<int>(100);
            writer.Written.Should().Be(0);
            writer.HasSome.Should().BeFalse();
            writer.WrittenSpan.Length.Should().Be(0);
            writer.WrittenMemory.Length.Should().Be(0);
            writer.WrittenSegment.Count.Should().Be(0);
        }

        [Fact]
        public void BufferWriter_Advance_Should_Change_Values_Of_Properties()
        {
            using var writer = new BufferWriter<int>(100);

            for (var i = 0; i < 100; i++)
            {
                writer.Advance(1);
                writer.HasSome.Should().BeTrue();
                writer.Written.Should().Be(i + 1);
                writer.WrittenSpan.Length.Should().Be(i + 1);
                writer.WrittenMemory.Length.Should().Be(i + 1);
                writer.WrittenSegment.Count.Should().Be(i + 1);
            }
        }

        [Fact]
        public void BufferWriter_Advance_Throw_On_Overflow()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var writer = new BufferWriter<int>(100);
                for (var i = 0; i < 250; i++) writer.Advance(1);
            });
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(100)]
        public void BufferWriter_Can_Advance(int step)
        {
            using var writer = new BufferWriter<int>(10 * step);
            for (var i = 0; i < 10; i++) writer.Advance(step);
        }

        [Fact]
        public void BufferWriter_Cant_Advance_On_Negative_Value()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var writer = new BufferWriter<int>(10);
                writer.Advance(-1);
            });
        }

        [Fact]
        public void BufferWriter_Can_Advance_On_Zero()
        {
            using var writer = new BufferWriter<int>(10);
            writer.Advance(0);
            writer.Written.Should().Be(0);
            writer.HasSome.Should().BeFalse();
        }

        [Fact]
        public void BufferWriter_Append_Should_Update_Properties()
        {
            using var writer = new BufferWriter<int>(10);

            for (var i = 0; i < 10; i++)
            {
                writer.Written.Should().Be(i);
                writer.WrittenSpan.Length.Should().Be(i);
                writer.WrittenMemory.Length.Should().Be(i);
                writer.WrittenSegment.Count.Should().Be(i);
                writer.GetSpan().Length.Should().Be(10 - i);
                writer.GetMemory().Span.Length.Should().Be(10 - i);
                writer.Append(i);
            }

            writer.HasSome.Should().BeTrue();
            
            writer.GetSpan().Length.Should().BeGreaterThan(10);
            writer.GetMemory().Span.Length.Should().BeGreaterThan(10);

            var writtenSpan = writer.WrittenSpan;
            for (var i = 0; i < writtenSpan.Length; i++) writtenSpan[i].Should().Be(i);

            var writtenMemory = writer.WrittenMemory;
            for (var i = 0; i < writtenMemory.Length; i++) writtenMemory.Span[i].Should().Be(i);

            var writtenSegment = writer.WrittenSegment;
            for (var i = 0; i < writtenSegment.Count; i++) writtenSegment[i].Should().Be(i);
        }

        [Fact]
        public void BufferWriter_Append_Should_Not_Throw_On_Overflow()
        {
            using var writer = new BufferWriter<int>(10);
            for (var i = 0; i < 500; i++) writer.Append(i);
        }

        [Fact]
        public void BufferWriter_GetMemory_Should_Throw_If_Negative()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var writer = new BufferWriter<int>(10);
                writer.GetMemory(-1);
            });
        }

        [Fact]
        public void BufferWriter_GetMemory_Should_Not_Throw_If_Too_Large()
        {
            using var writer = new BufferWriter<int>(10);
            writer.GetMemory(500);
        }

        [Fact]
        public void BufferWriter_GetMemory_Should_Return_Whole_Length()
        {
            using var writer = new BufferWriter<int>(10);
            var memory = writer.GetMemory(10);
            memory.Length.Should().Be(10);
        }

        [Fact]
        public void BufferWriter_GetSpan_Should_Throw_If_Negative()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var writer = new BufferWriter<int>(10);
                writer.GetSpan(-1);
            });
        }

        [Fact]
        public void BufferWriter_GetSpan_Should_Not_Throw_If_Too_Large()
        {
            using var writer = new BufferWriter<int>(10);
            writer.GetSpan(500);
        }

        [Fact]
        public void BufferWriter_GetSpan_Should_Return_Whole_Length()
        {
            using var writer = new BufferWriter<int>(10);
            var span = writer.GetSpan(10);
            span.Length.Should().Be(10);
        }

        [Fact]
        public void BufferWriter_GetSpan_Should_Shrink_And_Resize_If_Zero()
        {
            using var writer = new BufferWriter<int>(10);
            for (var i = 0; i < 10; i++)
            {
                writer.GetSpan().Length.Should().Be(10 - i);
                writer.Advance(1);
            }

            writer.GetSpan().Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void BufferWriter_GetMemory_Should_Shrink()
        {
            using var writer = new BufferWriter<int>(10);
            for (var i = 0; i < 10; i++)
            {
                writer.GetMemory().Length.Should().Be(10 - i);
                writer.Advance(1);
            }

            writer.GetMemory().Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public void BufferWriter_Clear_Should_Reset_BufferWriter()
        {
            using var writer = new BufferWriter<int>(10);
            for (var i = 0; i < 10; i++) writer.Advance(1);
            
            writer.Clear();
            
            writer.Written.Should().Be(0);
            writer.WrittenSpan.Length.Should().Be(0);
            writer.WrittenMemory.Length.Should().Be(0);
            writer.WrittenSegment.Count.Should().Be(0);
            writer.GetSpan().Length.Should().Be(10);
            writer.GetMemory().Length.Should().Be(10);


            for (var i = 0; i < 10; i++) writer.Append(i);
            for (var i = 0; i < 10; i++) writer.WrittenSpan[i].Should().Be(i);
        }

        [Fact]
        public void BufferWriter_AsEnumerable_Should_Enumerate_On_Empty()
        {
            var writer = new BufferWriter<int>(0);
            foreach (var v in writer.AsEnumerable()) v.Should().Be(0); // Won't be called
        }

        [Fact]
        public void BufferWriter_AsEnumerable_Should_Enumerate()
        {
            var writer = new BufferWriter<int>(10);

            var span = writer.GetSpan(10);
            for (var i = 0; i < span.Length; i++) span[i] = i;

            writer.Advance(10);

            var last = -1;
            foreach (var v in writer.AsEnumerable()) v.Should().Be(++last);
        }
    }
}
