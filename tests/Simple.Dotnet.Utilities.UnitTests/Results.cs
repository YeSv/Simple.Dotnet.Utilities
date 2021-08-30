namespace Simple.Dotnet.Utilities.UnitTests
{
    using System;
    using FluentAssertions;
    using Utilities.Results;
    using Xunit;

    public sealed class Results
    {
        [Fact]
        public void UniResult_Should_Be_Ok_By_Default()
        {
            var result = new UniResult<Unit, Exception>();
            result.IsOk.Should().BeTrue();
            result.Data.Should().BeNull();
        }

        [Fact]
        public void UniResult_Should_Be_Ok_If_Null()
        {
            var result = new UniResult<Unit, Exception>((Unit)null);
            result.IsOk.Should().BeTrue();
        }

        [Fact]
        public void UniResult_Should_Be_Not_Ok_If_Error()
        {
            var result = new UniResult<Unit, string>("Error");
            result.IsOk.Should().BeFalse();
            result.Data.Should().Be("Error");
        }

        [Fact]
        public void UniResult_Should_Have_HashCode_Of_Underlying_Data()
        {
            new UniResult<string, Exception>("Test").GetHashCode().Should().Be("Test".GetHashCode());
            new UniResult<Unit, Exception>(Unit.Shared).GetHashCode().Should().Be(Unit.Shared.GetHashCode());

            new UniResult<Unit, string>("Error").GetHashCode().Should().Be("Error".GetHashCode());

            var ex = new Exception();
            new UniResult<Unit, Exception>(ex).GetHashCode().Should().Be(ex.GetHashCode());
        }

        [Fact]
        public void UniResult_Should_Be_Equal()
        {
            new UniResult<Unit, Exception>().Equals(new UniResult<Unit, Exception>()).Should().BeTrue();
            new UniResult<Unit, Exception>(Unit.Shared).Equals(new UniResult<Unit, Exception>(Unit.Shared)).Should().BeTrue();
            new UniResult<string, Exception>("Data").Equals(new UniResult<string, Exception>("Data")).Should().BeTrue();
            new UniResult<Unit, string>("Error").Equals(new UniResult<Unit, string>("Error")).Should().BeTrue();
            new UniResult<object, Exception>("Test").Equals(new UniResult<object, Exception>("Test")).Should().BeTrue();
        }

        [Fact]
        public void UniResult_Should_Not_Be_Equal()
        {
            new UniResult<Unit, Exception>().Equals(new UniResult<Unit, Exception>(Unit.Shared)).Should().BeFalse();
            new UniResult<string, Exception>((string) null).Equals(new UniResult<string, Exception>(string.Empty)).Should().BeFalse();
            new UniResult<Unit, Exception>(new Exception()).Equals(new UniResult<Unit, Exception>(new Exception())).Should().BeFalse();
            new UniResult<Unit, Exception>(new Exception()).Equals(new UniResult<Unit, string>("Error")).Should().BeFalse();
            new UniResult<string, Exception>("data").Equals(new UniResult<string, Exception>("Data")).Should().BeFalse();
        }

        [Fact]
        public void UniResult_To_String_Should_Use_Underlying_Data()
        {
            new UniResult<Unit, string>().ToString().Should().Be("Result with null data");
            new UniResult<Unit, Exception>(Unit.Shared).ToString().Should().Be(Unit.Shared.ToString());
            new UniResult<Unit, Exception>(new Exception()).ToString().Should().Be(new Exception().ToString());
            new UniResult<Unit, string>("string").ToString().Should().Be("string");
            new UniResult<string, Exception>("data").ToString().Should().Be("data");
        }

        [Fact]
        public void UniResult_Cast_Should_Return_Underlying_Data()
        {
            ((Unit) new UniResult<Unit, Exception>(Unit.Shared)).Should().Be(Unit.Shared);
        }

        [Fact]
        public void UniResult_Ok_Should_Create_Success_Result()
        {
            UniResult.Ok<Unit, Exception>(Unit.Shared).IsOk.Should().BeTrue();
            UniResult.Ok<string, Exception>(string.Empty).IsOk.Should().BeTrue();
            UniResult.Ok<Unit, Exception>(null).IsOk.Should().BeTrue();
            UniResult.Ok<Exception, Exception>(null).IsOk.Should().BeTrue();
        }

        [Fact]
        public void UniResult_Error_Should_Create_Error_Result()
        {
            UniResult.Error<Unit, string>("Error").IsOk.Should().BeFalse();
            UniResult.Error<Unit, Exception>(new Exception()).IsOk.Should().BeFalse();
        }

        [Fact]
        public void ResultT_Should_Not_Be_Ok_By_Default()
        {
            var result = new Result<Unit, Exception>();
            result.IsOk.Should().BeFalse();
            result.Ok.Should().BeNull();
            result.Error.Should().BeNull();
        }

        [Fact]
        public void ResultT_Should_Be_Ok_If_Null()
        {
            var result = new Result<Unit, Exception>((Unit)null);
            result.IsOk.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Ok.Should().BeNull();
        }

        [Fact]
        public void ResultT_Should_Be_Not_Ok_If_Error()
        {
            var result = new Result<Unit, string>("Error");
            result.IsOk.Should().BeFalse();
            result.Ok.Should().BeNull();
            result.Error.Should().Be("Error");
        }

        [Fact]
        public void ResultT_Should_Have_HashCode_Of_Underlying_Data()
        {
            new Result<string, Exception>("Test").GetHashCode().Should().Be("Test".GetHashCode());
            new Result<Unit, Exception>(Unit.Shared).GetHashCode().Should().Be(Unit.Shared.GetHashCode());

            new Result<Unit, string>("Error").GetHashCode().Should().Be("Error".GetHashCode());

            var ex = new Exception();
            new Result<Unit, Exception>(ex).GetHashCode().Should().Be(ex.GetHashCode());
        }

        [Fact]
        public void ResultT_Should_Be_Equal()
        {
            new Result<Unit, Exception>().Equals(new Result<Unit, Exception>()).Should().BeTrue();
            new Result<Unit, Exception>(Unit.Shared).Equals(new Result<Unit, Exception>(Unit.Shared)).Should().BeTrue();
            new Result<string, Exception>("Data").Equals(new Result<string, Exception>("Data")).Should().BeTrue();
            new Result<Unit, string>("Error").Equals(new Result<Unit, string>("Error")).Should().BeTrue();
            new Result<object, string>("Test").Equals(new Result<object, string>("Test")).Should().BeTrue();

            var ex = new Exception();
            new Result<object, Exception>(ex).Equals(new Result<object, Exception>(ex)).Should().BeTrue();
            new Result<Unit, Exception>(ex).Equals(new Result<Unit, Exception>(ex)).Should().BeTrue();
        }

        [Fact]
        public void ResultT_Should_Not_Be_Equal()
        {
            new Result<Unit, Exception>().Equals(new Result<Unit, Exception>(Unit.Shared)).Should().BeFalse();
            new Result<string, Exception>((string)null).Equals(new Result<string, Exception>(string.Empty)).Should().BeFalse();
            new Result<Exception, string>(new Exception()).Equals(new Result<Exception, string>(new Exception())).Should().BeFalse();
            new Result<string, Exception>("data").Equals(new Result<string, Exception>("Data")).Should().BeFalse();
        }

        [Fact]
        public void ResultT_To_String_Should_Use_Underlying_Data()
        {
            new Result<Unit, Exception>().ToString().Should().Be("Result with null data");
            new Result<Unit, Exception>(Unit.Shared).ToString().Should().Be(Unit.Shared.ToString());
            new Result<Unit, Exception>(new Exception()).ToString().Should().Be(new Exception().ToString());
            new Result<Unit, string>("string").ToString().Should().Be("string");
            new Result<string, Exception>("data").ToString().Should().Be("data");
        }

        [Fact]
        public void ResultT_Cast_String_Should_Use_Underlying_Data()
        {
            ((string)new Result<Unit, Exception>()).Should().Be("Result with null data");
            ((string)new Result<Unit, Exception>(Unit.Shared)).Should().Be(Unit.Shared.ToString());
            ((string)new Result<Unit, Exception>(new Exception())).Should().Be(new Exception().ToString());
        }

        [Fact]
        public void ResultT_Cast_Error_Should_Use_Underlying_Error()
        {
            var ex = new Exception();
            ((Exception)new Result<Unit, Exception>(ex)).Should().Be(ex);

            ex = new ArgumentNullException();
            ((Exception)new Result<Unit, Exception>(ex)).Should().Be(ex);

            ex = new InvalidOperationException();
            ((Exception)new Result<Unit, Exception>(ex)).Should().Be(ex);
        }

        [Fact]
        public void ResultT_Ok_Should_Create_Success_Result()
        {
            Result.Ok<Unit, Exception>(Unit.Shared).IsOk.Should().BeTrue();
            Result.Ok<string, Exception>(string.Empty).IsOk.Should().BeTrue();
            Result.Ok<Unit, Exception>((Unit)null).IsOk.Should().BeTrue();
        }

        [Fact]
        public void ResultT_Error_Should_Create_Error_Result()
        {
            Result.Error<Unit, string>("Error").IsOk.Should().BeFalse();
            Result.Error<Unit, Exception>(new Exception()).IsOk.Should().BeFalse();
        }

        [Fact]
        public void Result_Can_Be_Deconstructed()
        {
            var (unit, error) = new Result<Unit, Exception>(Unit.Shared);
            error.Should().BeNull();
            unit.Should().Be(Unit.Shared);

            (unit, error) = new Result<Unit, Exception>(new Exception());
            unit.Should().BeNull();
            error.Should().NotBeNull();
        }

        [Fact]
        public void UniResult_AsResult_Should_Produce_Valid_Result()
        {
            new UniResult<Unit, Exception>(Unit.Shared).AsResult().Ok.Should().Be(Unit.Shared);
            new UniResult<Unit, Exception>(new Exception()).AsResult().Error.Should().NotBeNull();
        }

        [Fact]
        public void UniResult_Can_Be_Deconstructed()
        {
            var (unit, error) = new UniResult<Unit, Exception>(Unit.Shared);
            error.Should().BeNull();
            unit.Should().Be(Unit.Shared);

            (unit, error) = new UniResult<Unit, Exception>(new Exception());
            unit.Should().BeNull();
            error.Should().NotBeNull();
        }
    }
}
