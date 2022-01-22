namespace Simple.Dotnet.Utilities.UnitTests
{
    using System;
    using FluentAssertions;
    using Pools;
    using Xunit;

    public sealed class ValueRent
    {
        [Fact]
        public void ValueRent_With_Null_Should_Return_Null()
        {
            var checker = (object obj, object context) =>
            {
                obj.Should().BeNull();
                context.Should().BeNull();
            };

            using var rent = new ValueRent<object>(null, checker);
            rent.Value.Should().BeNull();

            using var contextRent = new ValueRent<object, object>(null, null, checker);
            contextRent.Value.Should().BeNull();
        }

        [Fact]
        public void ValueRent_Should_Not_Throw_On_Multiple_Dispose()
        {
            var checker = (object obj, object context) =>
            {
                obj.Should().BeNull();
                context.Should().BeNull();
            };

            var rent = new ValueRent<object>(null, checker);
            rent.Dispose();
            rent.Dispose();

            using var contextRent = new ValueRent<object, object>(null, null, checker);

            contextRent.Dispose();
            contextRent.Dispose();
        }

        [Fact]
        public void ValueRent_Should_Not_Throw_On_Multiple_Dispose_Non_Null()
        {
            var (calls, contextCalls) = (0, 0);

            var rent = new ValueRent<object>(new(), new(), (object obj, object context) =>
            {
                calls++;
                obj.Should().NotBeNull();
                context.Should().NotBeNull();
            });

            rent.Dispose();
            rent.Dispose();
            calls.Should().Be(1);

            var contextRent = new ValueRent<object, object>(new(), new(), (object obj, object context) =>
            {
                contextCalls++;
                obj.Should().NotBeNull();
                context.Should().NotBeNull();
            });

            contextRent.Dispose();
            contextRent.Dispose();
            contextCalls.Should().Be(1);
        }
    }
}
