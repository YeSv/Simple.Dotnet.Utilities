namespace Simple.Dotnet.Utilities.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Buffers;
    using FluentAssertions;
    using Xunit;

    public sealed class ArrayBufferWriter
    {
        [Fact]
        public void ArrayBufferWriter_Should_Be_Initialized_As_Empty()
        {
            var writer = new ArrayBufferWriter<int>(0);
            writer.Length.Should().Be(0);
            writer.Written.Should().Be(0);
            writer.HasSome.Should().BeFalse();
            writer.Available.Should().Be(0);
            writer.IsFull.Should().BeTrue();
            writer.WrittenSpan.Length.Should().Be(0);
            writer.WrittenMemory.Length.Should().Be(0);
            writer.WrittenSegment.Array.Length.Should().Be(0);
        }

        [Fact]
        public void ArrayBufferWriter_Should_Be_Initialized_As_Empty_Negative()
        {
            var writer = new ArrayBufferWriter<int>(-1);
            writer.Length.Should().Be(0);
            writer.Written.Should().Be(0);
            writer.HasSome.Should().BeFalse();
            writer.Available.Should().Be(0);
            writer.IsFull.Should().BeTrue();
            writer.WrittenSpan.Length.Should().Be(0);
            writer.WrittenMemory.Length.Should().Be(0);
            writer.WrittenSegment.Array.Length.Should().Be(0);
        }

        [Fact]
        public void ArrayBufferWriter_Written_Should_Be_Empty_If_Not_Used()
        {
            var writer = new ArrayBufferWriter<int>(100);

            writer.Length.Should().Be(100);
            writer.Written.Should().Be(0);
            writer.HasSome.Should().BeFalse();
            writer.Available.Should().Be(100);
            writer.IsFull.Should().BeFalse();
            writer.WrittenSpan.Length.Should().Be(0);
            writer.WrittenMemory.Length.Should().Be(0);
            writer.WrittenSegment.Count.Should().Be(0);
        }

        [Fact]
        public void ArrayBufferWriter_Advance_Should_Change_Values_Of_Properties()
        {
            var writer = new ArrayBufferWriter<int>(100);

            for (var i = 0; i < writer.Length; i++)
            {
                writer.Advance(1);
                writer.HasSome.Should().BeTrue();
                writer.Written.Should().Be(i + 1);
                writer.Available.Should().Be(writer.Length - (i + 1));
                writer.WrittenSpan.Length.Should().Be(i + 1);
                writer.WrittenMemory.Length.Should().Be(i + 1);
                writer.WrittenSegment.Count.Should().Be(i + 1);
            }

            writer.IsFull.Should().BeTrue();
        }

        [Fact]
        public void ArrayBufferWriter_Advance_Should_Throw_On_Overflow()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var writer = new ArrayBufferWriter<int>(100);
                for (var i = 0; i < 200; i++) writer.Advance(1);
            });
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(100)]
        public void ArrayBufferWriter_Can_Advance(int step)
        {
            var writer = new ArrayBufferWriter<int>(10 * step);
            for (var i = 0; i < 10; i++) writer.Advance(step);

            writer.IsFull.Should().BeTrue();
        }

        [Fact]
        public void ArrayBufferWriter_Cant_Advance_On_Negative_Value()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var writer = new ArrayBufferWriter<int>(10);
                writer.Advance(-1);
            });
        }

        [Fact]
        public void ArrayBufferWriter_Can_Advance_On_Zero()
        {
            var writer = new ArrayBufferWriter<int>(10);
            writer.Advance(0);
            writer.Written.Should().Be(0);
            writer.HasSome.Should().BeFalse();
        }

        [Fact]
        public void ArrayBufferWriter_Append_Should_Update_Properties()
        {
            var writer = new ArrayBufferWriter<int>(10);

            for (var i = 0; i < writer.Length; i++)
            {
                writer.Append(i);
                writer.Written.Should().Be(i + 1);
                writer.HasSome.Should().BeTrue();
                writer.Available.Should().Be(writer.Length - (i + 1));
                writer.WrittenSpan.Length.Should().Be(i + 1);
                writer.WrittenMemory.Length.Should().Be(i + 1);
                writer.WrittenSegment.Count.Should().Be(i + 1);
                writer.GetSpan().Length.Should().Be(writer.Length - (i + 1));
                writer.GetMemory().Span.Length.Should().Be(writer.Length - (i + 1));
            }

            writer.IsFull.Should().BeTrue();
            writer.GetSpan().Length.Should().Be(0);
            writer.GetMemory().Span.Length.Should().Be(0);

            var writtenSpan = writer.WrittenSpan;
            for (var i = 0; i < writtenSpan.Length; i++) writtenSpan[i].Should().Be(i);

            var writtenMemory = writer.WrittenMemory;
            for (var i = 0; i < writtenMemory.Length; i++) writtenMemory.Span[i].Should().Be(i);

            var writtenSegment = writer.WrittenSegment;
            for (var i = 0; i < writtenSegment.Count; i++) writtenSegment[i].Should().Be(i);
        }

        [Fact]
        public void ArrayBufferWriter_Append_Should_Throw_On_Overflow()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var writer = new ArrayBufferWriter<int>(10);
                for (var i = 0; i < writer.Length * 2; i++) writer.Append(i);
            });
        }

        [Fact]
        public void ArrayBufferWriter_GetMemory_Should_Throw_If_Negative()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var writer = new ArrayBufferWriter<int>(10);
                writer.GetMemory(-1);
            });
        }

        [Fact]
        public void ArrayBufferWriter_GetMemory_Should_Throw_If_Too_Large()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var writer = new ArrayBufferWriter<int>(10);
                writer.GetMemory(writer.Length + 1);
            });
        }

        [Fact]
        public void ArrayBufferWriter_GetMemory_Should_Return_Whole_Length()
        {
            var writer = new ArrayBufferWriter<int>(10);
            var memory = writer.GetMemory(writer.Length);
            memory.Length.Should().Be(writer.Length);
        }

        [Fact]
        public void ArrayBufferWriter_GetSpan_Should_Throw_If_Negative()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var writer = new ArrayBufferWriter<int>(10);
                writer.GetSpan(-1);
            });
        }

        [Fact]
        public void ArrayBufferWriter_GetSpan_Should_Throw_If_Too_Large()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var writer = new ArrayBufferWriter<int>(10);
                writer.GetSpan(writer.Length + 1);
            });
        }

        [Fact]
        public void ArrayBufferWriter_GetSpan_Should_Return_Whole_Length()
        {
            var writer = new ArrayBufferWriter<int>(10);
            var span = writer.GetSpan(writer.Length);
            span.Length.Should().Be(writer.Length);
        }

        [Fact]
        public void ArrayBufferWriter_GetSpan_Should_Shrink()
        {
            var writer = new ArrayBufferWriter<int>(10);
            for (var i = 0; i < writer.Length; i++)
            {
                writer.Advance(1);
                writer.GetSpan().Length.Should().Be(writer.Length - (i + 1));
            }

            writer.GetSpan().Length.Should().Be(0);
        }

        [Fact]
        public void ArrayBufferWriter_GetMemory_Should_Shrink()
        {
            var writer = new ArrayBufferWriter<int>(10);
            for (var i = 0; i < writer.Length; i++)
            {
                writer.Advance(1);
                writer.GetMemory().Length.Should().Be(writer.Length - (i + 1));
            }

            writer.GetMemory().Length.Should().Be(0);
        }

        [Fact]
        public void ArrayBufferWriter_Clear_Should_Reset_ArrayBufferWriter()
        {
            var writer = new ArrayBufferWriter<int>(10);
            for (var i = 0; i < writer.Length; i++) writer.Advance(1);

            writer.Available.Should().Be(0);
            writer.IsFull.Should().BeTrue();

            writer.Clear();

            writer.Available.Should().Be(writer.Length);
            writer.IsFull.Should().BeFalse();
            writer.Written.Should().Be(0);
            writer.WrittenSpan.Length.Should().Be(0);
            writer.WrittenMemory.Length.Should().Be(0);
            writer.WrittenSegment.Count.Should().Be(0);
            writer.GetSpan().Length.Should().Be(writer.Length);
            writer.GetMemory().Length.Should().Be(writer.Length);


            for (var i = 0; i < writer.Length; i++) writer.Append(i);
            for (var i = 0; i < writer.Length; i++) writer.WrittenSpan[i].Should().Be(i);
        }

        [Fact]
        public void ArrayBufferWriter_AsEnumerable_Should_Enumerate_On_Empty()
        {
            var writer = new ArrayBufferWriter<int>(0);
            foreach (var v in writer.AsEnumerable()) v.Should().Be(0); // Won't be called
        }

        [Fact]
        public void ArrayBufferWriter_AsEnumerable_Should_Enumerate()
        {
            var writer = new ArrayBufferWriter<int>(10);

            var span = writer.GetSpan(10);
            for (var i = 0; i < span.Length; i++) span[i] = i;

            writer.Advance(10);

            var last = -1;
            foreach (var v in writer.AsEnumerable()) v.Should().Be(++last);
        }
    }
}
