//==================================================================================================
// ResultExceptionExtensions unit tests - Testing Exception → Result conversion.
// Test suite for FromException methods and exception message formatting.
//==================================================================================================

namespace Fox.ResultKit.Tests;

//==================================================================================================
/// <summary>
/// ResultExceptionExtensions unit tests.
/// </summary>
//==================================================================================================
public sealed class ResultExceptionExtensionsTests
{
    #region FromException (basic)

    [Fact]
    public void FromException_should_create_failure_result()
    {
        var exception = new InvalidOperationException("test error");

        var result = ResultExceptionExtensions.FromException<int>(exception);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("test error");
    }

    [Fact]
    public void FromException_should_handle_different_exception_types()
    {
        var exception = new ArgumentException("argument error");

        var result = ResultExceptionExtensions.FromException<string>(exception);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("argument error");
    }

    [Fact]
    public void FromException_should_throw_when_exception_is_null()
    {
        var act = () => ResultExceptionExtensions.FromException<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region FromException (with inner exceptions)

    [Fact]
    public void FromException_should_include_inner_exceptions_when_specified()
    {
        var innerException = new InvalidOperationException("inner error");
        var exception = new ArgumentException("outer error", innerException);

        var result = ResultExceptionExtensions.FromException<int>(exception, includeInnerExceptions: true, includeStackTrace: false);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("outer error");
        result.Error.Should().Contain("Inner: inner error");
    }

    [Fact]
    public void FromException_should_not_include_inner_exceptions_by_default()
    {
        var innerException = new InvalidOperationException("inner error");
        var exception = new ArgumentException("outer error", innerException);

        var result = ResultExceptionExtensions.FromException<int>(exception);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("outer error");
        result.Error.Should().NotContain("inner error");
    }

    [Fact]
    public void FromException_should_handle_multiple_inner_exceptions()
    {
        var innermost = new InvalidOperationException("innermost");
        var middle = new ArgumentException("middle", innermost);
        var outer = new Exception("outer", middle);

        var result = ResultExceptionExtensions.FromException<string>(outer, includeInnerExceptions: true, includeStackTrace: false);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("outer");
        result.Error.Should().Contain("Inner: middle");
        result.Error.Should().Contain("Inner: innermost");
    }

    #endregion

    #region FromException (with stack trace)

    [Fact]
    public void FromException_should_include_stack_trace_when_specified()
    {
        Exception exception;
        try
        {
            throw new InvalidOperationException("test error");
        }
#pragma warning disable CA1031 // Do not catch general exception types - Intentional for test setup
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            exception = ex;
        }

        var result = ResultExceptionExtensions.FromException<int>(exception, includeInnerExceptions: false, includeStackTrace: true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("test error");
        result.Error.Should().Contain("Stack:");
        result.Error.Should().Contain("FromException_should_include_stack_trace_when_specified");
    }

    [Fact]
    public void FromException_should_not_include_stack_trace_by_default()
    {
        Exception exception;
        try
        {
            throw new InvalidOperationException("test error");
        }
#pragma warning disable CA1031 // Do not catch general exception types - Intentional for test setup
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            exception = ex;
        }

        var result = ResultExceptionExtensions.FromException<int>(exception);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("test error");
        result.Error.Should().NotContain("Stack:");
    }

    #endregion

    #region FromException (AggregateException)

    [Fact]
    public void FromException_should_handle_aggregate_exception()
    {
        var ex1 = new InvalidOperationException("error 1");
        var ex2 = new ArgumentException("error 2");
        var aggEx = new AggregateException("aggregate", ex1, ex2);

        var result = ResultExceptionExtensions.FromException<int>(aggEx);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error 1");
        result.Error.Should().Contain("error 2");
        result.Error.Should().Contain(";");
    }

    [Fact]
    public void FromException_should_flatten_nested_aggregate_exceptions()
    {
        var ex1 = new InvalidOperationException("error 1");
        var ex2 = new ArgumentException("error 2");
        var innerAgg = new AggregateException(ex1, ex2);
        var ex3 = new Exception("error 3");
        var outerAgg = new AggregateException(innerAgg, ex3);

        var result = ResultExceptionExtensions.FromException<string>(outerAgg);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("error 1");
        result.Error.Should().Contain("error 2");
        result.Error.Should().Contain("error 3");
    }

    #endregion

    #region FromException (combined options)

    [Fact]
    public void FromException_should_combine_inner_exceptions_and_stack_trace()
    {
        Exception exception;
        try
        {
            var innerException = new InvalidOperationException("inner error");
            throw new ArgumentException("outer error", innerException);
        }
#pragma warning disable CA1031 // Do not catch general exception types - Intentional for test setup
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            exception = ex;
        }

        var result = ResultExceptionExtensions.FromException<int>(exception, includeInnerExceptions: true, includeStackTrace: true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("outer error");
        result.Error.Should().Contain("Inner: inner error");
        result.Error.Should().Contain("Stack:");
    }

    #endregion
}
