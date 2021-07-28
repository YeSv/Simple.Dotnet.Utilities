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
            var result = new UniResult<Unit>();
            result.IsOk.Should().BeTrue();
            result.Data.Should().BeNull();
        }

        [Fact]
        public void UniResult_Should_Be_Ok_If_Null()
        {
            var result = new UniResult<Unit>(null);
            result.IsOk.Should().BeTrue();
        }

        [Fact]
        public void UniResult_Should_Be_Not_Ok_If_Error()
        {
            var result = new UniResult<Unit>("Error");
            result.IsOk.Should().BeFalse();
            result.Data.Should().Be("Error");
        }

        [Fact]
        public void UniResult_Should_Have_HashCode_Of_Underlying_Data()
        {
            new UniResult<string>("Test").GetHashCode().Should().Be("Test".GetHashCode());
            new UniResult<Unit>(Unit.Shared).GetHashCode().Should().Be(Unit.Shared.GetHashCode());

            new UniResult<Unit>("Error").GetHashCode().Should().Be("Error".GetHashCode());

            var ex = new Exception();
            new UniResult<Unit>(ex).GetHashCode().Should().Be(ex.GetHashCode());
        }

        [Fact]
        public void UniResult_Should_Be_Equal()
        {
            new UniResult<Unit>().Equals(new UniResult<Unit>()).Should().BeTrue();
            new UniResult<Unit>(Unit.Shared).Equals(new UniResult<Unit>(Unit.Shared)).Should().BeTrue();
            new UniResult<string>("Data").Equals(new UniResult<string>("Data")).Should().BeTrue();
            new UniResult<Unit>("Error").Equals(new UniResult<Unit>("Error")).Should().BeTrue();
            new UniResult<object>("Test").Equals(new UniResult<object>("Test")).Should().BeTrue();
        }

        [Fact]
        public void UniResult_Should_Not_Be_Equal()
        {
            new UniResult<Unit>().Equals(new UniResult<Unit>(Unit.Shared)).Should().BeFalse();
            new UniResult<string>((string) null).Equals(new UniResult<string>(string.Empty)).Should().BeFalse();
            new UniResult<Exception>(new Exception()).Equals(new UniResult<Exception>(new Exception())).Should().BeFalse();
            new UniResult<Unit>(new Exception()).Equals(new UniResult<Unit>("Error")).Should().BeFalse();
            new UniResult<string>("data").Equals(new UniResult<string>("Data")).Should().BeFalse();
        }

        [Fact]
        public void UniResult_To_String_Should_Use_Underlying_Data()
        {
            new UniResult<Unit>().ToString().Should().Be("Result with null data");
            new UniResult<Unit>(Unit.Shared).ToString().Should().Be(Unit.Shared.ToString());
            new UniResult<Unit>(new Exception()).ToString().Should().Be(new Exception().ToString());
            new UniResult<Unit>("string").ToString().Should().Be("string");
            new UniResult<string>("data").ToString().Should().Be("data");
        }

        [Fact]
        public void UniResult_Cast_String_Should_Use_Underlying_Data()
        {
            ((string)new UniResult<Unit>()).Should().Be("Result with null data");
            ((string)new UniResult<Unit>(Unit.Shared)).Should().Be(Unit.Shared.ToString());
            ((string)new UniResult<Unit>(new Exception())).Should().Be(new Exception().ToString());
            ((string)new UniResult<Unit>("string")).Should().Be("string");
        }

        [Fact]
        public void UniResult_Cast_Exception_Should_Use_Underlying_Data()
        {
            var ex = new Exception();
            ((Exception) new UniResult<Unit>(ex)).Should().Be(ex);

            ex = new ArgumentNullException();
            ((Exception) new UniResult<Unit>(ex)).Should().Be(ex);

            ex = new InvalidOperationException();
            ((Exception)new UniResult<Unit>(ex)).Should().Be(ex);
        }

        [Fact]
        public void UniResult_Cast_Exception_Should_Throw_If_Not_An_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var ex = ((Exception) new UniResult<Unit>(Unit.Shared));
                ex.Should().BeNull(); // Won't be called
            });
        }

        [Fact]
        public void UniResult_Cast_Should_Return_Underlying_Data()
        {
            ((Unit) new UniResult<Unit>(Unit.Shared)).Should().Be(Unit.Shared);
        }

        [Fact]
        public void UniResult_Ok_Should_Create_Success_Result()
        {
            UniResult.Ok(Unit.Shared).IsOk.Should().BeTrue();
            UniResult.Ok(string.Empty).IsOk.Should().BeTrue();
            UniResult.Ok<Unit>(null).IsOk.Should().BeTrue();
        }

        [Fact]
        public void UniResult_Error_Should_Create_Error_Result()
        {
            UniResult.Error<Unit>("Error").IsOk.Should().BeFalse();
            UniResult.Error<Unit>(new Exception()).IsOk.Should().BeFalse();
        }

        [Fact]
        public void Result_Should_Be_Ok_By_Default()
        {
            var result = new Result<Unit>();
            result.IsOk.Should().BeTrue();
            result.Data.Should().BeNull();
            result.Error.Should().BeNull();
        }
        
        [Fact]
        public void Result_Should_Be_Ok_If_Null()
        {
            var result = new Result<Unit>(null);
            result.IsOk.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Data.Should().BeNull();
        }

        [Fact]
        public void Result_Should_Be_Not_Ok_If_Error()
        {
            var result = new Result<Unit>("Error");
            result.IsOk.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Error.Should().Be("Error");
        }

        [Fact]
        public void Result_Should_Have_HashCode_Of_Underlying_Data()
        {
            new Result<string>("Test").GetHashCode().Should().Be("Test".GetHashCode());
            new Result<Unit>(Unit.Shared).GetHashCode().Should().Be(Unit.Shared.GetHashCode());

            new Result<Unit>("Error").GetHashCode().Should().Be("Error".GetHashCode());

            var ex = new Exception();
            new Result<Unit>(ex).GetHashCode().Should().Be(ex.GetHashCode());
        }

        [Fact]
        public void Result_Should_Be_Equal()
        {
            new Result<Unit>().Equals(new Result<Unit>()).Should().BeTrue();
            new Result<Unit>(Unit.Shared).Equals(new Result<Unit>(Unit.Shared)).Should().BeTrue();
            new Result<string>("Data").Equals(new Result<string>("Data")).Should().BeTrue();
            new Result<Unit>("Error").Equals(new Result<Unit>("Error")).Should().BeTrue();
            new Result<object>("Test").Equals(new Result<object>("Test")).Should().BeTrue();

            var ex = new Exception();
            new Result<object>(ex).Equals(new Result<object>(ex)).Should().BeTrue();
            new Result<Unit>(ex).Equals(new Result<Unit>(ex)).Should().BeTrue();
        }

        [Fact]
        public void Result_Should_Not_Be_Equal()
        {
            new Result<Unit>().Equals(new Result<Unit>(Unit.Shared)).Should().BeFalse();
            new Result<string>((string)null).Equals(new Result<string>(string.Empty)).Should().BeFalse();
            new Result<Exception>(new Exception()).Equals(new Result<Exception>(new Exception())).Should().BeFalse();
            new Result<Unit>(new Exception()).Equals(new Result<Unit>("Error")).Should().BeFalse();
            new Result<string>("data").Equals(new Result<string>("Data")).Should().BeFalse();
        }

        [Fact]
        public void Result_To_String_Should_Use_Underlying_Data()
        {
            new Result<Unit>().ToString().Should().Be("Result with null data");
            new Result<Unit>(Unit.Shared).ToString().Should().Be(Unit.Shared.ToString());
            new Result<Unit>(new Exception()).ToString().Should().Be(new Exception().ToString());
            new Result<Unit>("string").ToString().Should().Be("string");
            new Result<string>("data").ToString().Should().Be("data");
        }

        [Fact]
        public void Result_Cast_String_Should_Use_Underlying_Data()
        {
            ((string)new Result<Unit>()).Should().Be("Result with null data");
            ((string)new Result<Unit>(Unit.Shared)).Should().Be(Unit.Shared.ToString());
            ((string)new Result<Unit>(new Exception())).Should().Be(new Exception().ToString());
            ((string)new Result<Unit>("string")).Should().Be("string");
        }

        [Fact]
        public void Result_Cast_Exception_Should_Use_Underlying_Data()
        {
            var ex = new Exception();
            ((Exception)new Result<Unit>(ex)).Should().Be(ex);

            ex = new ArgumentNullException();
            ((Exception)new Result<Unit>(ex)).Should().Be(ex);

            ex = new InvalidOperationException();
            ((Exception)new Result<Unit>(ex)).Should().Be(ex);
        }

        [Fact]
        public void Result_Cast_Exception_Should_Throw_If_Not_An_Exception()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var ex = ((Exception)new Result<Unit>(Unit.Shared));
                ex.Should().BeNull(); // Won't be called
            });
        }

        [Fact]
        public void Result_Cast_Should_Return_Underlying_Data()
        {
            ((Unit)new Result<Unit>(Unit.Shared)).Should().Be(Unit.Shared);
        }

        [Fact]
        public void Result_Ok_Should_Create_Success_Result()
        {
            Result.Ok(Unit.Shared).IsOk.Should().BeTrue();
            Result.Ok(string.Empty).IsOk.Should().BeTrue();
            Result.Ok<Unit>(null).IsOk.Should().BeTrue();
        }

        [Fact]
        public void Result_Error_Should_Create_Error_Result()
        {
            Result.Error<Unit>("Error").IsOk.Should().BeFalse();
            Result.Error<Unit>(new Exception()).IsOk.Should().BeFalse();
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
        public void ResultT_Cast_Should_Return_Underlying_Data()
        {
            ((Unit)new Result<Unit>(Unit.Shared)).Should().Be(Unit.Shared);
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
        public void Unwrap_Should_Return_Ok()
        {
            new UniResult<Unit>(Unit.Shared).Unwrap().Should().Be(Unit.Shared);
            new UniResult<string>(string.Empty).Unwrap().Should().Be(string.Empty);
            new UniResult<Unit>(null).Unwrap().Should().BeNull();
        }

        [Fact]
        public void Unwrap_Should_Throw_On_Error()
        {
            Assert.Throws<InvalidCastException>(() => new UniResult<Unit>("error").Unwrap());
        }

        [Fact]
        public void To_Should_Produce_Valid_ResultT()
        {
            new UniResult<Unit>(Unit.Shared).To<Unit, Exception>().IsOk.Should().BeTrue();
            new UniResult<string>(string.Empty).To<string, Exception>().IsOk.Should().BeTrue();
            new UniResult<Unit>(new Exception()).To<Unit, Exception>().IsOk.Should().BeFalse();
            new UniResult<string>(new Exception()).To<string, Exception>().IsOk.Should().BeFalse();
        }

        [Fact]
        public void To_Should_Throw_If_Mismatch()
        {
            Assert.Throws<InvalidOperationException>(() => new UniResult<Unit>(new Exception()).To<Unit, string>());
        }

        [Fact]
        public void Error_UniResult_Should_Return_Error()
        {
            new UniResult<Unit>(new Exception()).Error<Unit, Exception>().Should().NotBeNull();
            new UniResult<Unit>("Error").Error<Unit, string>().Should().Be("Error");
        }

        [Fact]
        public void Error_UniResult_Should_Throw()
        {
            Assert.Throws<InvalidCastException>(() => new UniResult<Unit>(new Exception()).Error<Unit, string>());
        }

        [Fact]
        public void Error_UniResult_Should_Throw_IsOk()
        {
            Assert.Throws<InvalidOperationException>(() => new UniResult<Unit>(Unit.Shared).Error<Unit, string>());
        }

        [Fact]
        public void Error_Result_Should_Return_Error()
        {
            new Result<Unit>(new Exception()).Error<Unit, Exception>().Should().NotBeNull();
            new Result<Unit>("Error").Error<Unit, string>().Should().Be("Error");
        }

        [Fact]
        public void Error_Result_Should_Throw()
        {
            Assert.Throws<InvalidCastException>(() => new Result<Unit>(new Exception()).Error<Unit, string>());
        }

        [Fact]
        public void Error_Result_Should_Throw_Is_Ok()
        {
            Assert.Throws<InvalidOperationException>(() => new Result<Unit>(Unit.Shared).Error<Unit, string>());
        }
    }
}
