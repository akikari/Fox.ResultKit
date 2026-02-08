//==================================================================================================
// Result unit tests - Testing the Result (non-generic) class.
// Test suite for non-generic Result pattern implementation.
//==================================================================================================

namespace Fox.ResultKit.Tests;

//==================================================================================================
/// <summary>
/// Result (non-generic) unit tests.
/// </summary>
//==================================================================================================
public sealed class ResultTests
{
    [Fact]
    public void Success_should_create_success_result()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_should_create_failure_result()
    {
        var result = Result.Failure("error message");

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("error message");
    }

    [Fact]
    public void IsSuccess_and_IsFailure_should_reflect_state()
    {
        var success = Result.Success();
        var failure = Result.Failure("error");

        success.IsSuccess.Should().BeTrue();
        success.IsFailure.Should().BeFalse();

        failure.IsSuccess.Should().BeFalse();
        failure.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ThrowIfFailure_should_throw_on_failure()
    {
        var result = Result.Failure("error message");

        result.Invoking(r => r.ThrowIfFailure())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("error message");
    }

    [Fact]
    public void ThrowIfFailure_should_not_throw_on_success()
    {
        var result = Result.Success();

        result.Invoking(r => r.ThrowIfFailure()).Should().NotThrow();
    }

    [Fact]
    public void Failure_should_throw_when_error_is_null()
    {
        var act = () => Result.Failure(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Failure_should_throw_when_error_is_empty()
    {
        var act = () => Result.Failure(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Failure_should_throw_when_error_is_whitespace()
    {
        var act = () => Result.Failure("   ");

        act.Should().Throw<ArgumentException>();
    }
}

