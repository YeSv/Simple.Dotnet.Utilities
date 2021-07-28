namespace Simple.Dotnet.Utilities.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Tasks;
    using Xunit;


    public sealed class TaskBuffers
    {
        [Fact]
        public void TaskBuffer_Should_Be_Initialized_As_Empty()
        {
            var buffer = new TaskBuffer(0);
            buffer.Written.Should().Be(0);
            buffer.HasSome.Should().BeFalse();
            buffer.Available.Should().Be(0);
            buffer.IsFull.Should().BeTrue();
            buffer.WrittenSpan.Length.Should().Be(0);
            buffer.WrittenMemory.Length.Should().Be(0);
            buffer.WrittenSegment.Array.Length.Should().Be(0);
        }

        [Fact]
        public void TaskBuffer_Should_Be_Initialized_As_Empty_Negative()
        {
            var buffer = new TaskBuffer(-1);
            buffer.Written.Should().Be(0);
            buffer.HasSome.Should().BeFalse();
            buffer.Available.Should().Be(0);
            buffer.IsFull.Should().BeTrue();
            buffer.WrittenSpan.Length.Should().Be(0);
            buffer.WrittenMemory.Length.Should().Be(0);
            buffer.WrittenSegment.Array.Length.Should().Be(0);
        }

        [Fact]
        public void TaskBuffer_Written_Should_Be_Empty_If_Not_Used()
        {
            var buffer = new TaskBuffer(100);

            buffer.Written.Should().Be(0);
            buffer.HasSome.Should().BeFalse();
            buffer.Available.Should().Be(100);
            buffer.IsFull.Should().BeFalse();
            buffer.WrittenSpan.Length.Should().Be(0);
            buffer.WrittenMemory.Length.Should().Be(0);
            buffer.WrittenSegment.Count.Should().Be(0);
        }

        [Fact]
        public void TaskBuffer_Advance_Should_Change_Values_Of_Properties()
        {
            var buffer = new TaskBuffer(100);

            for (var i = 0; i < buffer.Length; i++)
            {
                buffer.Advance(1);
                buffer.HasSome.Should().BeTrue();
                buffer.Written.Should().Be(i + 1);
                buffer.Available.Should().Be(buffer.Length - (i + 1));
                buffer.WrittenSpan.Length.Should().Be(i + 1);
                buffer.WrittenMemory.Length.Should().Be(i + 1);
                buffer.WrittenSegment.Count.Should().Be(i + 1);
            }

            buffer.IsFull.Should().BeTrue();
        }

        [Fact]
        public void TaskBuffer_Advance_Should_Throw_On_Overflow()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer(100);
                for (var i = 0; i < 200; i++) buffer.Advance(1);
            });
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(100)]
        public void TaskBuffer_Can_Advance(int step)
        {
            var buffer = new TaskBuffer(10 * step);
            for (var i = 0; i < 10; i++) buffer.Advance(step);

            buffer.IsFull.Should().BeTrue();
        }

        [Fact]
        public void TaskBuffer_Cant_Advance_On_Negative_Value()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer(10);
                buffer.Advance(-1);
            });
        }

        [Fact]
        public void TaskBuffer_Can_Advance_On_Zero()
        {
            var buffer = new TaskBuffer(10);
            buffer.Advance(0);
            buffer.Written.Should().Be(0);
            buffer.HasSome.Should().BeFalse();
        }

        [Fact]
        public void TaskBuffer_Append_Should_Update_Properties()
        {
            var buffer = new TaskBuffer(10);

            for (var i = 0; i < buffer.Length; i++)
            {
                buffer.Append(Task.FromResult(i));
                buffer.Written.Should().Be(i + 1);
                buffer.HasSome.Should().BeTrue();
                buffer.Available.Should().Be(buffer.Length - (i + 1));
                buffer.WrittenSpan.Length.Should().Be(i + 1);
                buffer.WrittenMemory.Length.Should().Be(i + 1);
                buffer.WrittenSegment.Count.Should().Be(i + 1);
                buffer.GetSpan().Length.Should().Be(buffer.Length - (i + 1));
                buffer.GetMemory().Span.Length.Should().Be(buffer.Length - (i + 1));
            }

            buffer.IsFull.Should().BeTrue();
            buffer.GetSpan().Length.Should().Be(0);
            buffer.GetMemory().Span.Length.Should().Be(0);

            var writtenSpan = buffer.WrittenSpan;
            for (var i = 0; i < writtenSpan.Length; i++) ((Task<int>)writtenSpan[i]).Result.Should().Be(i);

            var writtenMemory = buffer.WrittenMemory;
            for (var i = 0; i < writtenMemory.Length; i++) ((Task<int>)writtenMemory.Span[i]).Result.Should().Be(i);

            var writtenSegment = buffer.WrittenSegment;
            for (var i = 0; i < writtenSegment.Count; i++) ((Task<int>)writtenSegment[i]).Result.Should().Be(i);
        }

        [Fact]
        public void TaskBuffer_Append_Should_Throw_On_Overflow()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer(10);
                for (var i = 0; i < buffer.Length * 2; i++) buffer.Append(Task.CompletedTask);
            });
        }

        [Fact]
        public void TaskBuffer_GetMemory_Should_Throw_If_Negative()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer(10);
                buffer.GetMemory(-1);
            });
        }

        [Fact]
        public void TaskBuffer_GetMemory_Should_Throw_If_Too_Large()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer(10);
                buffer.GetMemory(buffer.Length + 1);
            });
        }

        [Fact]
        public void TaskBuffer_GetMemory_Should_Return_Whole_Length()
        {
            var buffer = new TaskBuffer(10);
            var memory = buffer.GetMemory(buffer.Length);
            memory.Length.Should().Be(buffer.Length);
        }

        [Fact]
        public void TaskBuffer_GetSpan_Should_Throw_If_Negative()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer(10);
                buffer.GetSpan(-1);
            });
        }

        [Fact]
        public void TaskBuffer_GetSpan_Should_Throw_If_Too_Large()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer(10);
                buffer.GetSpan(buffer.Length + 1);
            });
        }

        [Fact]
        public void TaskBuffer_GetSpan_Should_Return_Whole_Length()
        {
            var buffer = new TaskBuffer(10);
            var span = buffer.GetSpan(buffer.Length);
            span.Length.Should().Be(buffer.Length);
        }

        [Fact]
        public void TaskBuffer_GetSpan_Should_Shrink()
        {
            var buffer = new TaskBuffer(10);
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer.Advance(1);
                buffer.GetSpan().Length.Should().Be(buffer.Length - (i + 1));
            }

            buffer.GetSpan().Length.Should().Be(0);
        }

        [Fact]
        public void TaskBuffer_GetMemory_Should_Shrink()
        {
            var buffer = new TaskBuffer(10);
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer.Advance(1);
                buffer.GetMemory().Length.Should().Be(buffer.Length - (i + 1));
            }

            buffer.GetMemory().Length.Should().Be(0);
        }

        [Fact]
        public void TaskBuffer_Should_Work_With_WaitAll_Not_Full()
        {
            var buffer = new TaskBuffer(1_000);
            for (var i = 0; i < 10; i++) buffer.Append(Task.CompletedTask);

            Task.WaitAll((Task[])buffer);
            Task.WhenAll((Task[])buffer).Wait();
        }

        [Fact]
        public void TaskBuffer_Should_Work_With_WaitAll_Full()
        {
            var buffer = new TaskBuffer(1_000);
            for (var i = 0; i < buffer.Length; i++) buffer.Append(Task.FromResult(i));

            Task.WaitAll((Task[])buffer);
            Task.WhenAll((Task[])buffer).Wait();
        }

        [Fact]
        public void TaskBuffer_Should_Work_With_WaitAll_After_Clear()
        {
            var buffer = new TaskBuffer(1_000);
            for (var i = 0; i < buffer.Length; i++) buffer.Append(Task.FromResult(i));

            Task.WaitAll((Task[])buffer);
            Task.WhenAll((Task[])buffer).Wait();

            buffer.Clear();

            Task.WaitAll((Task[])buffer);
            Task.WhenAll((Task[])buffer).Wait();
        }

        [Fact]
        public void TaskBufferT_Should_Be_Initialized_As_Empty()
        {
            var buffer = new TaskBuffer<int>(0);
            buffer.Written.Should().Be(0);
            buffer.HasSome.Should().BeFalse();
            buffer.Available.Should().Be(0);
            buffer.IsFull.Should().BeTrue();
            buffer.WrittenSpan.Length.Should().Be(0);
            buffer.WrittenMemory.Length.Should().Be(0);
            buffer.WrittenSegment.Array.Length.Should().Be(0);
        }

        [Fact]
        public void TaskBufferT_Should_Be_Initialized_As_Empty_Negative()
        {
            var buffer = new TaskBuffer<int>(-1);
            buffer.Written.Should().Be(0);
            buffer.HasSome.Should().BeFalse();
            buffer.Available.Should().Be(0);
            buffer.IsFull.Should().BeTrue();
            buffer.WrittenSpan.Length.Should().Be(0);
            buffer.WrittenMemory.Length.Should().Be(0);
            buffer.WrittenSegment.Array.Length.Should().Be(0);
        }

        [Fact]
        public void TaskBufferT_Written_Should_Be_Empty_If_Not_Used()
        {
            var buffer = new TaskBuffer<int>(100);

            buffer.Written.Should().Be(0);
            buffer.HasSome.Should().BeFalse();
            buffer.Available.Should().Be(100);
            buffer.IsFull.Should().BeFalse();
            buffer.WrittenSpan.Length.Should().Be(0);
            buffer.WrittenMemory.Length.Should().Be(0);
            buffer.WrittenSegment.Count.Should().Be(0);
        }

        [Fact]
        public void TaskBufferT_Advance_Should_Change_Values_Of_Properties()
        {
            var buffer = new TaskBuffer<int>(100);

            for (var i = 0; i < buffer.Length; i++)
            {
                buffer.Advance(1);
                buffer.HasSome.Should().BeTrue();
                buffer.Written.Should().Be(i + 1);
                buffer.Available.Should().Be(buffer.Length - (i + 1));
                buffer.WrittenSpan.Length.Should().Be(i + 1);
                buffer.WrittenMemory.Length.Should().Be(i + 1);
                buffer.WrittenSegment.Count.Should().Be(i + 1);
            }

            buffer.IsFull.Should().BeTrue();
        }

        [Fact]
        public void TaskBufferT_Advance_Should_Throw_On_Overflow()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer<int>(100);
                for (var i = 0; i < 200; i++) buffer.Advance(1);
            });
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(100)]
        public void TaskBufferT_Can_Advance(int step)
        {
            var buffer = new TaskBuffer<int>(10 * step);
            for (var i = 0; i < 10; i++) buffer.Advance(step);

            buffer.IsFull.Should().BeTrue();
        }

        [Fact]
        public void TaskBufferT_Cant_Advance_On_Negative_Value()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer<int>(10);
                buffer.Advance(-1);
            });
        }

        [Fact]
        public void TaskBufferT_Can_Advance_On_Zero()
        {
            var buffer = new TaskBuffer<int>(10);
            buffer.Advance(0);
            buffer.Written.Should().Be(0);
            buffer.HasSome.Should().BeFalse();
        }

        [Fact]
        public void TaskBufferT_Append_Should_Update_Properties()
        {
            var buffer = new TaskBuffer<int>(10);

            for (var i = 0; i < buffer.Length; i++)
            {
                buffer.Append(Task.FromResult(i));
                buffer.Written.Should().Be(i + 1);
                buffer.HasSome.Should().BeTrue();
                buffer.Available.Should().Be(buffer.Length - (i + 1));
                buffer.WrittenSpan.Length.Should().Be(i + 1);
                buffer.WrittenMemory.Length.Should().Be(i + 1);
                buffer.WrittenSegment.Count.Should().Be(i + 1);
                buffer.GetSpan().Length.Should().Be(buffer.Length - (i + 1));
                buffer.GetMemory().Span.Length.Should().Be(buffer.Length - (i + 1));
            }

            buffer.IsFull.Should().BeTrue();
            buffer.GetSpan().Length.Should().Be(0);
            buffer.GetMemory().Span.Length.Should().Be(0);

            var writtenSpan = buffer.WrittenSpan;
            for (var i = 0; i < writtenSpan.Length; i++) ((Task<int>)writtenSpan[i]).Result.Should().Be(i);

            var writtenMemory = buffer.WrittenMemory;
            for (var i = 0; i < writtenMemory.Length; i++) ((Task<int>)writtenMemory.Span[i]).Result.Should().Be(i);

            var writtenSegment = buffer.WrittenSegment;
            for (var i = 0; i < writtenSegment.Count; i++) ((Task<int>)writtenSegment[i]).Result.Should().Be(i);
        }

        [Fact]
        public void TaskBufferT_Append_Should_Throw_On_Overflow()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer<int>(10);
                for (var i = 0; i < buffer.Length * 2; i++) buffer.Append(Task.FromResult(i));
            });
        }

        [Fact]
        public void TaskBufferT_GetMemory_Should_Throw_If_Negative()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer<int>(10);
                buffer.GetMemory(-1);
            });
        }

        [Fact]
        public void TaskBufferT_GetMemory_Should_Throw_If_Too_Large()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer<int>(10);
                buffer.GetMemory(buffer.Length + 1);
            });
        }

        [Fact]
        public void TaskBufferT_GetMemory_Should_Return_Whole_Length()
        {
            var buffer = new TaskBuffer<int>(10);
            var memory = buffer.GetMemory(buffer.Length);
            memory.Length.Should().Be(buffer.Length);
        }

        [Fact]
        public void TaskBufferT_GetSpan_Should_Throw_If_Negative()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer<int>(10);
                buffer.GetSpan(-1);
            });
        }

        [Fact]
        public void TaskBufferT_GetSpan_Should_Throw_If_Too_Large()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var buffer = new TaskBuffer<int>(10);
                buffer.GetSpan(buffer.Length + 1);
            });
        }

        [Fact]
        public void TaskBufferT_GetSpan_Should_Return_Whole_Length()
        {
            var buffer = new TaskBuffer<int>(10);
            var span = buffer.GetSpan(buffer.Length);
            span.Length.Should().Be(buffer.Length);
        }

        [Fact]
        public void TaskBufferT_GetSpan_Should_Shrink()
        {
            var buffer = new TaskBuffer<int>(10);
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer.Advance(1);
                buffer.GetSpan().Length.Should().Be(buffer.Length - (i + 1));
            }

            buffer.GetSpan().Length.Should().Be(0);
        }

        [Fact]
        public void TaskBufferT_GetMemory_Should_Shrink()
        {
            var buffer = new TaskBuffer<int>(10);
            for (var i = 0; i < buffer.Length; i++)
            {
                buffer.Advance(1);
                buffer.GetMemory().Length.Should().Be(buffer.Length - (i + 1));
            }

            buffer.GetMemory().Length.Should().Be(0);
        }

        [Fact]
        public void TaskBufferT_Should_Work_With_WaitAll_Not_Full()
        {
            var buffer = new TaskBuffer<int>(1_000);
            for (var i = 0; i < 10; i++) buffer.Append(Task.FromResult(i));

            Task.WaitAll((Task[])buffer);
            Task.WhenAll((Task[])buffer).Wait();
        }

        [Fact]
        public void TaskBufferT_Should_Work_With_WaitAll_Full()
        {
            var buffer = new TaskBuffer<int>(1_000);
            for (var i = 0; i < buffer.Length; i++) buffer.Append(Task.FromResult(i));

            Task.WaitAll((Task[])buffer);
            Task.WhenAll((Task[])buffer).Wait();
        }

        [Fact]
        public void TaskBufferT_Should_Work_With_WaitAll_After_Clear()
        {
            var buffer = new TaskBuffer<int>(1_000);
            for (var i = 0; i < buffer.Length; i++) buffer.Append(Task.FromResult(i));

            Task.WaitAll((Task[])buffer);
            Task.WhenAll((Task[])buffer).Wait();

            buffer.Clear();


            Task.WaitAll((Task[])buffer);
            Task.WhenAll((Task[])buffer).Wait();
        }
    }
}
