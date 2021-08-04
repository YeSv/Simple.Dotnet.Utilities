namespace Simple.Dotnet.Utilities.UnitTests
{
    using System;
    using FluentAssertions;
    using Xunit;


    public sealed class ArrayStack
    {
        [Fact]
        public void ArrayStack_Should_Push_And_Pop_Value()
        {
            var arrayStack = new ArrayStack<int>(stackalloc int[10]);
            
            for (var i = 0; i < arrayStack.Length; i++) arrayStack.Push(i);

            arrayStack.IsFull.Should().BeTrue();
            arrayStack.Length.Should().Be(10);
            arrayStack.WrittenSpan.Length.Should().Be(10);

            var writtenSpan = arrayStack.WrittenSpan;
            for (var i = 0; i < writtenSpan.Length; i++) writtenSpan[i].Should().Be(i);

            for (var i = writtenSpan.Length - 1; i >= 0; i--) arrayStack.Pop().Should().Be(i);

            arrayStack.IsFull.Should().BeFalse();
            arrayStack.WrittenSpan.Length.Should().Be(0);
        }

        [Fact]
        public void ArrayStack_Should_Reset()
        {
            var arrayStack = new ArrayStack<int>(stackalloc int[10]);

            for (var i = 0; i < arrayStack.Length; i++) arrayStack.Push(i);

            arrayStack.Reset();

            arrayStack.IsFull.Should().BeFalse();
            arrayStack.WrittenSpan.Length.Should().Be(0);
        }

        [Fact]
        public void ArrayStack_Should_Throw_On_Overflow()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var arrayStack = new ArrayStack<int>(stackalloc int[10]);
                for (var i = 0; i < 20; i++) arrayStack.Push(i);
            });
        }

        [Fact]
        public void ArrayStack_Should_Throw_On_Empty_Pop()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var arrayStack = new ArrayStack<int>(stackalloc int[10]);
                arrayStack.Push(1);
                arrayStack.Pop();
                arrayStack.Push(1);
                arrayStack.Pop();
                arrayStack.Pop();
            });

            Assert.Throws<InvalidOperationException>(() =>
            {
                var arrayStack = new ArrayStack<int>(stackalloc int[10]);
                arrayStack.Pop();
            });
        }
    }
}
