//==================================================================================================
// ResultCombineExtensions unit tests - Testing Result combination.
// Test suite for Combine methods for multi-validation aggregation.
//==================================================================================================

namespace Fox.ResultKit.Tests;

//==================================================================================================
/// <summary>
/// ResultCombineExtensions unit tests.
/// </summary>
//==================================================================================================
public sealed class ResultCombineExtensionsTests
{
    #region Combine (non-generic)

    [Fact]
    public void Combine_should_return_success_when_all_results_succeed()
    {
        var result1 = Result.Success();
        var result2 = Result.Success();
        var result3 = Result.Success();

        var combined = ResultCombineExtensions.Combine(result1, result2, result3);

        combined.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Combine_should_return_first_failure()
    {
        var result1 = Result.Success();
        var result2 = Result.Failure("first error");
        var result3 = Result.Failure("second error");

        var combined = ResultCombineExtensions.Combine(result1, result2, result3);

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("first error");
    }

    [Fact]
    public void Combine_should_return_success_for_empty_array()
    {
        var combined = ResultCombineExtensions.Combine();

        combined.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Combine_should_throw_when_results_is_null()
    {
        var act = () => ResultCombineExtensions.Combine(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Combine (generic with external value)

    [Fact]
    public void Combine_with_value_should_return_success_when_all_results_succeed()
    {
        var result1 = Result.Success();
        var result2 = Result.Success();
        var result3 = Result.Success();

        var combined = ResultCombineExtensions.Combine(42, result1, result2, result3);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be(42);
    }

    [Fact]
    public void Combine_with_value_should_return_first_failure()
    {
        var result1 = Result.Success();
        var result2 = Result.Failure("first error");
        var result3 = Result.Failure("second error");

        var combined = ResultCombineExtensions.Combine("test", result1, result2, result3);

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("first error");
    }

    [Fact]
    public void Combine_with_value_should_return_success_for_empty_array()
    {
        var combined = ResultCombineExtensions.Combine(100);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be(100);
    }

    [Fact]
    public void Combine_with_value_should_throw_when_results_is_null()
    {
        var act = () => ResultCombineExtensions.Combine(42, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Combine (generic with last result value)

    [Fact]
    public void Combine_last_value_should_return_success_when_all_results_succeed()
    {
        var result1 = Result<int>.Success(10);
        var result2 = Result<int>.Success(20);
        var result3 = Result<int>.Success(30);

        var combined = ResultCombineExtensions.Combine(result1, result2, result3);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be(30); // Utolsó érték
    }

    [Fact]
    public void Combine_last_value_should_return_first_failure()
    {
        var result1 = Result<string>.Success("first");
        var result2 = Result<string>.Failure("first error");
        var result3 = Result<string>.Failure("second error");

        var combined = ResultCombineExtensions.Combine(result1, result2, result3);

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("first error");
    }

    [Fact]
    public void Combine_last_value_should_throw_for_empty_array()
    {
        var act = () => ResultCombineExtensions.Combine<int>();

        act.Should().Throw<ArgumentException>()
            .WithMessage("At least one result is required.*");
    }

    [Fact]
    public void Combine_last_value_should_throw_when_results_is_null()
    {
        var act = () => ResultCombineExtensions.Combine<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
