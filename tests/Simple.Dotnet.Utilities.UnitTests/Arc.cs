namespace Simple.Dotnet.Utilities.UnitTests
{
    using System;
    using FluentAssertions;
    using Utilities.Arc;
    using Utilities.Results;
    using Xunit;

    public sealed class Arc
    {
        [Fact]
        public void Arc_Should_Dispose_Only_Once()
        {
            var resource = new Resource();
            var arc = new Arc<Resource>(resource);
            var rent1 = arc.Rent(); // 1

            arc.Rent().Dispose(); // 2
            arc.Rent().Dispose(); // 3

            rent1.Value.Should().NotBeNull();
            rent1.Dispose();

            resource.IsDisposed.Should().BeTrue();
            resource.Times.Should().Be(1);
        }

        [Fact]
        public void Arc_Should_Call_Action_On_Drop()
        {
            var called = false;
            var resource = new Resource();
            var arc = new Arc<Resource>(resource, a => called = true);

            var rent1 = arc.Rent(); // 1

            arc.Rent().Dispose(); // 2
            arc.Rent().Dispose(); // 3

            rent1.Value.Should().NotBeNull();
            rent1.Dispose();

            resource.IsDisposed.Should().BeTrue();
            resource.Times.Should().Be(1);
            called.Should().BeTrue();
        }


        [Fact]
        public void Arc_Should_Throw_If_No_Referers()
        {
            var arc = new Arc<Unit>(Unit.Shared);

            arc.Rent().Dispose();
            Assert.Throws<InvalidOperationException>(() => arc.Rent());
        }
    }


    public sealed class Resource : IDisposable
    {
        public int Times { get; set; }
        public bool IsDisposed { get; set; }


        public void Dispose()
        {
            IsDisposed = true;
            Times++;
        }
    }
}
