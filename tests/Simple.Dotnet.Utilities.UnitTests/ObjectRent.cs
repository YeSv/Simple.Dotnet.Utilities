namespace Simple.Dotnet.Utilities.UnitTests
{
    using System;
    using FluentAssertions;
    using Microsoft.Extensions.ObjectPool;
    using Pools;
    using Xunit;

    public sealed class ObjectRent
    {
        [Fact]
        public void ObjectRent_Without_Pool_Should_Return_Nulls()
        {
            using var rent = new ObjectRent<object>(null);
            rent.Value.Should().BeNull();
        }

        [Fact]
        public void ObjectRent_Should_Not_Throw_On_Multiple_Dispose()
        {
            var rent = new ObjectRent<object>(null);
            rent.Dispose();
            rent.Dispose();
        }

        [Fact]
        public void ObjectRent_Should_Not_Throw_On_Multiple_Dispose_Non_Null()
        {
            var rent = new ObjectRent<object>(new DefaultObjectPool<object>(new DefaultPooledObjectPolicy<object>()));
            rent.Dispose();
            rent.Dispose();
        }

        [Fact]
        public void ObjectRent_Should_Get_Value_From_Pool_Only_Once()
        {
            using var rent = new ObjectRent<object>(new OneValuePool());

            var oldValue = rent.Value;
            var newValue = rent.Value;

            oldValue.Should().NotBeNull();
            newValue.Should().NotBeNull();
            oldValue.Should().Be(newValue);
        }

        [Fact]
        public void ObjectRent_Should_Return_Value_Only_Once()
        {
            var rent = new ObjectRent<object>(new OneValuePool());
            rent.Value.Should().NotBeNull();
            rent.Value.Should().NotBeNull();

            for (var i = 0; i < 100; i++) rent.Dispose();
        }

        [Fact]
        public void ObjectRent_Action_Should_Be_Called()
        {
            var rent = new ObjectRent<PoolObject>(new DefaultObjectPool<PoolObject>(new DefaultPooledObjectPolicy<PoolObject>()), o =>
            {
                o.Value = 0;
            });

            var value = rent.Value;
            value.Value = 10;

            rent.Dispose();
            value.Value.Should().Be(0);
        }

        public sealed class OneValuePool : ObjectPool<object>
        {
            object _obj = new object();

            public override object Get()
            {
                if (_obj == null) throw new Exception("OneValuePool");
                var obj = _obj;
                _obj = null;
                return obj;
            }

            public override void Return(object obj)
            {
                if (_obj != null) throw new InvalidOperationException("OneValuePool");
                _obj = obj;
            }
        }

        public sealed class PoolObject
        {
            public int Value { get; set; }
        }
    }
}
