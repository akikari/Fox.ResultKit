//==================================================================================================
// Result<T> unit tests - Testing the Result<T> (generic) class.
// Test suite for generic Result pattern implementation.
//==================================================================================================

namespace Fox.ResultKit.Tests;

//==================================================================================================
/// <summary>
/// Result&lt;T&gt; (generic) unit tests.
/// </summary>
//==================================================================================================
public sealed class ResultOfTTests
{
    [Fact]
    public void Success_should_store_value()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_should_store_error()
    {
        var result = Result<int>.Failure("error message");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("error message");
    }

    [Fact]
    public void Value_should_throw_on_failure()
    {
        var result = Result<int>.Failure("error");

        result.Invoking(r => _ = r.Value)
            .Should().Throw<InvalidOperationException>()
            .WithMessage("error");
    }

    [Fact]
    public void IsSuccess_and_IsFailure_should_reflect_state()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure("error");

        success.IsSuccess.Should().BeTrue();
        success.IsFailure.Should().BeFalse();

        failure.IsSuccess.Should().BeFalse();
        failure.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Failure_should_throw_when_error_is_null()
    {
        var act = () => Result<int>.Failure(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Failure_should_throw_when_error_is_empty()
    {
        var act = () => Result<int>.Failure(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Failure_should_throw_when_error_is_whitespace()
    {
        var act = () => Result<int>.Failure("   ");

        act.Should().Throw<ArgumentException>();
    }
}
