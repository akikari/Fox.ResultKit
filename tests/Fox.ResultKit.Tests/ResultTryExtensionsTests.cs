//==================================================================================================
// ResultTryExtensions unit tests - Testing Try/TryAsync exception safety.
// Test suite for exception-safe operation execution methods.
//==================================================================================================

namespace Fox.ResultKit.Tests;

//==================================================================================================
/// <summary>
/// ResultTryExtensions unit tests.
/// </summary>
//==================================================================================================
public sealed class ResultTryExtensionsTests
{
    #region Try

    [Fact]
    public void Try_should_return_success_when_func_succeeds()
    {
        var result = ResultTryExtensions.Try(() => 42, "error");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Try_should_return_failure_when_func_throws()
    {
        var result = ResultTryExtensions.Try<int>(() => throw new InvalidOperationException("test exception"), "operation failed");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("operation failed");
    }

    [Fact]
    public void Try_should_catch_different_exception_types()
    {
        var result = ResultTryExtensions.Try<int>(() => throw new ArgumentException("argument error"), "operation failed");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("operation failed");
    }

    [Fact]
    public void Try_should_throw_when_func_is_null()
    {
        var act = () => ResultTryExtensions.Try<int>(null!, "error");

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region TryAsync

    [Fact]
    public async Task TryAsync_should_return_success_when_func_succeeds()
    {
        var result = await ResultTryExtensions.TryAsync(() => Task.FromResult(42), "error");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task TryAsync_should_return_failure_when_func_throws()
    {
        var result = await ResultTryExtensions.TryAsync<int>(
            () => throw new InvalidOperationException("test exception"),
            "operation failed"
        );

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("operation failed");
    }

    [Fact]
    public async Task TryAsync_should_catch_different_exception_types()
    {
        var result = await ResultTryExtensions.TryAsync<int>(
            () => throw new ArgumentException("argument error"),
            "operation failed"
        );

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("operation failed");
    }

    [Fact]
    public async Task TryAsync_should_throw_when_func_is_null()
    {
        var act = async () => await ResultTryExtensions.TryAsync<int>(null!, "error");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion
}
