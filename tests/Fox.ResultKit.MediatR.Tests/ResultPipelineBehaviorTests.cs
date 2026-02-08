//==================================================================================================
// ResultPipelineBehavior unit tests - Testing MediatR pipeline behavior.
// Test suite for automatic Result handling in MediatR request pipeline.
//==================================================================================================

namespace Fox.ResultKit.MediatR.Tests;

//==================================================================================================
/// <summary>
/// ResultPipelineBehavior unit tests.
/// </summary>
//==================================================================================================
public class ResultPipelineBehaviorTests
{
    //==============================================================================================
    /// <summary>
    /// On successful Result, the pipeline passes through the result.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task Handle_should_return_success_result_when_handler_succeeds()
    {
        var behavior = new ResultPipelineBehavior<TestRequest, Result>();
        var expectedResult = Result.Success();
        Task<Result> next() => Task.FromResult(expectedResult);

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.Should().Be(expectedResult);
        result.IsSuccess.Should().BeTrue();
    }

    //==============================================================================================
    /// <summary>
    /// On exception, returns Result.Failure for Result-typed responses.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task Handle_should_convert_exception_to_result_failure_for_result_type()
    {
        var behavior = new ResultPipelineBehavior<TestRequest, Result>();
        var exceptionMessage = "Test exception";
        Task<Result> next() => throw new InvalidOperationException(exceptionMessage);

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(exceptionMessage);
    }

    //==============================================================================================
    /// <summary>
    /// On exception, returns Result&lt;T&gt;.Failure for generic Result-typed responses.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task Handle_should_convert_exception_to_generic_result_failure_for_result_of_t_type()
    {
        var behavior = new ResultPipelineBehavior<TestRequest, Result<int>>();
        var exceptionMessage = "Test exception";
        Task<Result<int>> next() => throw new InvalidOperationException(exceptionMessage);

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(exceptionMessage);
    }

    //==============================================================================================
    /// <summary>
    /// On successful Result&lt;T&gt;, the pipeline passes through the result.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task Handle_should_return_success_result_of_t_when_handler_succeeds()
    {
        var behavior = new ResultPipelineBehavior<TestRequest, Result<string>>();
        var expectedValue = "Success";
        var expectedResult = Result<string>.Success(expectedValue);
        Task<Result<string>> next() => Task.FromResult(expectedResult);

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.Should().Be(expectedResult);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedValue);
    }

    //==============================================================================================
    /// <summary>
    /// For non-Result response types, the exception is rethrown.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task Handle_should_rethrow_exception_for_non_result_response_type()
    {
        var behavior = new ResultPipelineBehavior<TestRequest, string>();
        var exceptionMessage = "Test exception";
        Task<string> next() => throw new InvalidOperationException(exceptionMessage);

        var act = async () => await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(exceptionMessage);
    }

    //==============================================================================================
    /// <summary>
    /// Null next delegate esetén ArgumentNullException-t dob.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task Handle_should_throw_argument_null_exception_when_next_is_null()
    {
        var behavior = new ResultPipelineBehavior<TestRequest, Result>();

        var act = async () => await behavior.Handle(new TestRequest(), null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    //==============================================================================================
    /// <summary>
    /// Különböző kivétel típusok esetén Result.Failure-t ad vissza.
    /// </summary>
    //==============================================================================================
    [Theory]
    [InlineData(typeof(InvalidOperationException))]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(Exception))]
    public async Task Handle_should_convert_any_exception_type_to_result_failure(Type exceptionType)
    {
        ArgumentNullException.ThrowIfNull(exceptionType);

        var behavior = new ResultPipelineBehavior<TestRequest, Result>();
        var exceptionMessage = $"Test {exceptionType.Name}";
        var exception = (Exception)Activator.CreateInstance(exceptionType, exceptionMessage)!;
        Task<Result> next() => throw exception;

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(exceptionMessage);
    }

    //==============================================================================================
    /// <summary>
    /// Result&lt;string&gt; típusú válasz esetén Exception-t Result.Failure-re konvertálja.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task Handle_should_convert_exception_to_result_of_string_failure()
    {
        var behavior = new ResultPipelineBehavior<TestRequest, Result<string>>();
        var exceptionMessage = "String test exception";
        Task<Result<string>> next() => throw new InvalidOperationException(exceptionMessage);

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(exceptionMessage);
    }

    //==============================================================================================
    /// <summary>
    /// Result&lt;Guid&gt; típusú válasz esetén Exception-t Result.Failure-re konvertálja.
    /// </summary>
    //==============================================================================================
    [Fact]
    public async Task Handle_should_convert_exception_to_result_of_guid_failure()
    {
        var behavior = new ResultPipelineBehavior<TestRequest, Result<Guid>>();
        var exceptionMessage = "Guid test exception";
        Task<Result<Guid>> next() => throw new InvalidOperationException(exceptionMessage);

        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(exceptionMessage);
    }

    //==============================================================================================
    /// <summary>
    /// Teszt request.
    /// </summary>
    //==============================================================================================
    private record TestRequest : IRequest<Result>;
}
