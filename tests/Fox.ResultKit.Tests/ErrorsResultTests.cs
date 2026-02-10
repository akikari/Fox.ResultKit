//==================================================================================================
// Unit tests for ErrorsResult functionality.
// Tests for error aggregation, collection, and conversion methods.
//==================================================================================================

namespace Fox.ResultKit.Tests;

//==================================================================================================
/// <summary>
/// Unit tests for <see cref="ErrorsResult"/>.
/// </summary>
//==================================================================================================
public class ErrorsResultTests
{
    #region Success creation

    [Fact]
    public void Success_should_create_successful_result_with_no_errors()
    {
        var result = ErrorsResult.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Collect - non-generic Result

    [Fact]
    public void Collect_should_return_success_when_all_results_are_successful()
    {
        var result1 = Result.Success();
        var result2 = Result.Success();
        var result3 = Result.Success();

        var collected = ErrorsResult.Collect(result1, result2, result3);

        collected.IsSuccess.Should().BeTrue();
        collected.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Collect_should_aggregate_all_errors_from_failed_results()
    {
        var result1 = Result.Failure("Error 1");
        var result2 = Result.Success();
        var result3 = Result.Failure("Error 2");
        var result4 = Result.Failure("Error 3");

        var collected = ErrorsResult.Collect(result1, result2, result3, result4);

        collected.IsFailure.Should().BeTrue();
        collected.Errors.Should().HaveCount(3);
        collected.Errors.Should().Contain("Error 1");
        collected.Errors.Should().Contain("Error 2");
        collected.Errors.Should().Contain("Error 3");
    }

    [Fact]
    public void Collect_should_preserve_error_order()
    {
        var result1 = Result.Failure("First error");
        var result2 = Result.Failure("Second error");
        var result3 = Result.Failure("Third error");

        var collected = ErrorsResult.Collect(result1, result2, result3);

        collected.Errors[0].Should().Be("First error");
        collected.Errors[1].Should().Be("Second error");
        collected.Errors[2].Should().Be("Third error");
    }

    [Fact]
    public void Collect_should_throw_when_results_is_null()
    {
        Result[] nullResults = null!;

        var act = () => ErrorsResult.Collect(nullResults);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Collect_should_work_with_ResultError_formatted_errors()
    {
        var result1 = Result.Failure(ResultError.Create("VALIDATION_EMAIL", "Invalid email"));
        var result2 = Result.Failure(ResultError.Create("VALIDATION_PASSWORD", "Password too short"));

        var collected = ErrorsResult.Collect(result1, result2);

        collected.IsFailure.Should().BeTrue();
        collected.Errors.Should().HaveCount(2);
        collected.Errors[0].Should().Be("VALIDATION_EMAIL: Invalid email");
        collected.Errors[1].Should().Be("VALIDATION_PASSWORD: Password too short");
    }

    #endregion

    #region Collect - generic Result<T>

    [Fact]
    public void Collect_generic_should_return_success_when_all_results_are_successful()
    {
        var result1 = Result<int>.Success(1);
        var result2 = Result<int>.Success(2);
        var result3 = Result<int>.Success(3);

        var collected = ErrorsResult.Collect(result1, result2, result3);

        collected.IsSuccess.Should().BeTrue();
        collected.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Collect_generic_should_aggregate_all_errors_from_failed_results()
    {
        var result1 = Result<int>.Failure("Parse error 1");
        var result2 = Result<int>.Success(42);
        var result3 = Result<int>.Failure("Parse error 2");

        var collected = ErrorsResult.Collect(result1, result2, result3);

        collected.IsFailure.Should().BeTrue();
        collected.Errors.Should().HaveCount(2);
        collected.Errors.Should().Contain("Parse error 1");
        collected.Errors.Should().Contain("Parse error 2");
    }

    [Fact]
    public void Collect_generic_should_throw_when_results_is_null()
    {
        Result<string>[] nullResults = null!;

        var act = () => ErrorsResult.Collect(nullResults);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ToResult conversion

    [Fact]
    public void ToResult_should_return_success_when_no_errors()
    {
        var errorsResult = ErrorsResult.Success();

        var result = errorsResult.ToResult();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToResult_should_combine_errors_with_newlines()
    {
        var result1 = Result.Failure("Error 1");
        var result2 = Result.Failure("Error 2");
        var result3 = Result.Failure("Error 3");

        var errorsResult = ErrorsResult.Collect(result1, result2, result3);
        var result = errorsResult.ToResult();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"Error 1{Environment.NewLine}Error 2{Environment.NewLine}Error 3");
    }

    [Fact]
    public void ToResult_should_preserve_ResultError_format()
    {
        var result1 = Result.Failure(ResultError.Create("CODE_1", "Message 1"));
        var result2 = Result.Failure(ResultError.Create("CODE_2", "Message 2"));

        var errorsResult = ErrorsResult.Collect(result1, result2);
        var result = errorsResult.ToResult();

        result.Error.Should().Contain("CODE_1: Message 1");
        result.Error.Should().Contain("CODE_2: Message 2");
    }

    #endregion

    #region IsFailure property

    [Fact]
    public void IsFailure_should_be_opposite_of_IsSuccess()
    {
        var success = ErrorsResult.Success();
        var failure = ErrorsResult.Collect(Result.Failure("Error"));

        success.IsFailure.Should().BeFalse();
        failure.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Real-world scenario

    [Fact]
    public void Should_collect_multiple_validation_errors_in_user_registration_scenario()
    {
        // Simulate form validation
        var emailValidation = Result.Failure(ResultError.Create("VALIDATION_EMAIL_REQUIRED", "Email is required"));
        var passwordValidation = Result.Failure(ResultError.Create("VALIDATION_PASSWORD_LENGTH", "Password must be at least 8 characters"));
        var ageValidation = Result.Failure(ResultError.Create("VALIDATION_AGE_MINIMUM", "Must be at least 18 years old"));
        var termsValidation = Result.Failure(ResultError.Create("VALIDATION_TERMS_REQUIRED", "You must accept terms"));

        var validation = ErrorsResult.Collect(
            emailValidation,
            passwordValidation,
            ageValidation,
            termsValidation
        );

        validation.IsFailure.Should().BeTrue();
        validation.Errors.Should().HaveCount(4);

        var parsedErrors = validation.Errors.Select(ResultError.Parse).ToList();
        parsedErrors.Should().Contain(e => e.Code == "VALIDATION_EMAIL_REQUIRED");
        parsedErrors.Should().Contain(e => e.Code == "VALIDATION_PASSWORD_LENGTH");
        parsedErrors.Should().Contain(e => e.Code == "VALIDATION_AGE_MINIMUM");
        parsedErrors.Should().Contain(e => e.Code == "VALIDATION_TERMS_REQUIRED");
    }

    #endregion

    #region Mixed types (IResult interface)

    [Fact]
    public void Collect_should_support_mixed_result_and_generic_result_types()
    {
        var emailValidation = Result.Failure(ResultError.Create("VALIDATION_EMAIL_FORMAT", "Invalid email"));
        var ageValidation = Result<int>.Failure("Age must be positive");
        var nameValidation = Result<string>.Success("John Doe");
        var statusValidation = Result.Success();

        var collected = ErrorsResult.Collect(emailValidation, ageValidation, nameValidation, statusValidation);

        collected.IsFailure.Should().BeTrue();
        collected.Errors.Should().HaveCount(2);
        collected.Errors[0].Should().Be("VALIDATION_EMAIL_FORMAT: Invalid email");
        collected.Errors[1].Should().Be("Age must be positive");
    }

    [Fact]
    public void Collect_should_return_success_when_all_mixed_types_are_successful()
    {
        var validation1 = Result.Success();
        var validation2 = Result<int>.Success(42);
        var validation3 = Result<string>.Success("test");

        var collected = ErrorsResult.Collect(validation1, validation2, validation3);

        collected.IsSuccess.Should().BeTrue();
        collected.Errors.Should().BeEmpty();
    }

    #endregion

    #region Edge cases

    [Fact]
    public void Collect_should_throw_when_results_array_is_null()
    {
        IResult[] nullResults = null!;

        var act = () => ErrorsResult.Collect(nullResults);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("results");
    }

    [Fact]
    public void Collect_should_return_success_when_empty_array_is_provided()
    {
        var collected = ErrorsResult.Collect([]);

        collected.IsSuccess.Should().BeTrue();
        collected.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Collect_should_handle_single_result()
    {
        var singleResult = Result.Failure("Single error");

        var collected = ErrorsResult.Collect(singleResult);

        collected.IsFailure.Should().BeTrue();
        collected.Errors.Should().ContainSingle().Which.Should().Be("Single error");
    }

    #endregion
}
