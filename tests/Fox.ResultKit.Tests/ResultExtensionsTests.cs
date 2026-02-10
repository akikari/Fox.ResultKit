//==================================================================================================
// ResultExtensions unit tests - Testing Result functional extensions.
// Test suite for Map, Bind, Ensure, Tap, Match, ToResult extension methods.
//==================================================================================================

namespace Fox.ResultKit.Tests;

//==================================================================================================
/// <summary>
/// ResultExtensions unit tests.
/// </summary>
//==================================================================================================
public sealed class ResultExtensionsTests
{
    #region Map

    [Fact]
    public void Map_should_transform_success_value()
    {
        var result = Result<int>.Success(5);

        var mapped = result.Map(x => x * 2);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_should_propagate_failure()
    {
        var result = Result<int>.Failure("error");

        var mapped = result.Map(x => x * 2);

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be("error");
    }

    [Fact]
    public void Map_should_throw_when_result_is_null()
    {
        Result<int> result = null!;

        var act = () => result.Map(x => x * 2);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Map_should_throw_when_func_is_null()
    {
        var result = Result<int>.Success(5);

        var act = () => result.Map<int, int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Bind

    [Fact]
    public void Bind_should_chain_success_operations()
    {
        var result = Result<int>.Success(5);

        var bound = result.Bind(x => Result<string>.Success(x.ToString()));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_should_propagate_first_failure()
    {
        var result = Result<int>.Failure("first error");

        var bound = result.Bind(x => Result<string>.Success(x.ToString()));

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("first error");
    }

    [Fact]
    public void Bind_should_propagate_second_failure()
    {
        var result = Result<int>.Success(5);

        var bound = result.Bind(x => Result<string>.Failure("second error"));

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("second error");
    }

    [Fact]
    public void Bind_should_throw_when_result_is_null()
    {
        Result<int> result = null!;

        var act = () => result.Bind(x => Result<string>.Success(x.ToString()));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Bind_should_throw_when_func_is_null()
    {
        var result = Result<int>.Success(5);

        var act = () => result.Bind<int, string>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Bind_non_generic_should_chain_success_operations()
    {
        var result = Result.Success();

        var bound = result.Bind(() => Result.Success());

        bound.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Bind_non_generic_should_propagate_first_failure()
    {
        var result = Result.Failure("first error");

        var bound = result.Bind(() => Result.Success());

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("first error");
    }

    [Fact]
    public void Bind_non_generic_should_propagate_second_failure()
    {
        var result = Result.Success();

        var bound = result.Bind(() => Result.Failure("second error"));

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("second error");
    }

    [Fact]
    public void Bind_non_generic_should_throw_when_result_is_null()
    {
        Result result = null!;

        var act = () => result.Bind(() => Result.Success());

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Bind_non_generic_should_throw_when_func_is_null()
    {
        var result = Result.Success();

        var act = () => result.Bind(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Ensure

    [Fact]
    public void Ensure_should_pass_when_predicate_is_true()
    {
        var result = Result<int>.Success(10);

        var ensured = result.Ensure(x => x > 5, "value too small");

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(10);
    }

    [Fact]
    public void Ensure_should_fail_when_predicate_is_false()
    {
        var result = Result<int>.Success(3);

        var ensured = result.Ensure(x => x > 5, "value too small");

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().Be("value too small");
    }

    [Fact]
    public void Ensure_should_propagate_failure()
    {
        var result = Result<int>.Failure("original error");

        var ensured = result.Ensure(x => x > 5, "value too small");

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().Be("original error");
    }

    [Fact]
    public void Ensure_should_throw_when_result_is_null()
    {
        Result<int> result = null!;

        var act = () => result.Ensure(x => x > 5, "error");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ensure_should_throw_when_predicate_is_null()
    {
        var result = Result<int>.Success(10);

        var act = () => result.Ensure(null!, "error");

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Tap

    [Fact]
    public void Tap_should_execute_action_on_success()
    {
        var result = Result<int>.Success(10);
        var executed = false;

        var tapped = result.Tap(x => executed = true);

        executed.Should().BeTrue();
        tapped.Should().Be(result);
    }

    [Fact]
    public void Tap_should_not_execute_action_on_failure()
    {
        var result = Result<int>.Failure("error");
        var executed = false;

        var tapped = result.Tap(x => executed = true);

        executed.Should().BeFalse();
        tapped.Should().Be(result);
    }

    [Fact]
    public void Tap_should_throw_when_result_is_null()
    {
        Result<int> result = null!;

        var act = () => result.Tap(x => { });

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Tap_should_throw_when_action_is_null()
    {
        var result = Result<int>.Success(10);

        var act = () => result.Tap(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region TapFailure

    [Fact]
    public void TapFailure_should_execute_action_on_failure()
    {
        var result = Result<int>.Failure("test error");
        var capturedError = string.Empty;

        var tapped = result.TapFailure(error => capturedError = error);

        capturedError.Should().Be("test error");
        tapped.Should().Be(result);
    }

    [Fact]
    public void TapFailure_should_not_execute_action_on_success()
    {
        var result = Result<int>.Success(10);
        var executed = false;

        var tapped = result.TapFailure(error => executed = true);

        executed.Should().BeFalse();
        tapped.Should().Be(result);
    }

    [Fact]
    public void TapFailure_should_throw_when_result_is_null()
    {
        Result<int> result = null!;

        var act = () => result.TapFailure(error => { });

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TapFailure_should_throw_when_action_is_null()
    {
        var result = Result<int>.Failure("error");

        var act = () => result.TapFailure(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Match

    [Fact]
    public void Match_should_call_onSuccess_when_success()
    {
        var result = Result<int>.Success(10);

        var matched = result.Match(
            onSuccess: x => $"Success: {x}",
            onFailure: e => $"Error: {e}"
        );

        matched.Should().Be("Success: 10");
    }

    [Fact]
    public void Match_should_call_onFailure_when_failure()
    {
        var result = Result<int>.Failure("test error");

        var matched = result.Match(
            onSuccess: x => $"Success: {x}",
            onFailure: e => $"Error: {e}"
        );

        matched.Should().Be("Error: test error");
    }

    [Fact]
    public void Match_should_throw_when_result_is_null()
    {
        Result<int> result = null!;

        var act = () => result.Match(x => x.ToString(), e => e);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Match_should_throw_when_onSuccess_is_null()
    {
        var result = Result<int>.Success(10);

        var act = () => result.Match<int, string>(null!, e => e);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Match_should_throw_when_onFailure_is_null()
    {
        var result = Result<int>.Success(10);

        var act = () => result.Match<int, string>(x => x.ToString(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region MapAsync

    [Fact]
    public async Task MapAsync_should_transform_success_value()
    {
        var resultTask = Task.FromResult(Result<int>.Success(5));

        var mapped = await resultTask.MapAsync(x => Task.FromResult(x * 2));

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public async Task MapAsync_should_propagate_failure()
    {
        var resultTask = Task.FromResult(Result<int>.Failure("error"));

        var mapped = await resultTask.MapAsync(x => Task.FromResult(x * 2));

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be("error");
    }

    [Fact]
    public async Task MapAsync_should_throw_when_func_is_null()
    {
        var resultTask = Task.FromResult(Result<int>.Success(5));

        var act = async () => await resultTask.MapAsync<int, int>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region BindAsync

    [Fact]
    public async Task BindAsync_should_chain_success_operations()
    {
        var resultTask = Task.FromResult(Result<int>.Success(5));

        var bound = await resultTask.BindAsync(x => Task.FromResult(Result<string>.Success(x.ToString())));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public async Task BindAsync_should_propagate_first_failure()
    {
        var resultTask = Task.FromResult(Result<int>.Failure("first error"));

        var bound = await resultTask.BindAsync(x => Task.FromResult(Result<string>.Success(x.ToString())));

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("first error");
    }

    [Fact]
    public async Task BindAsync_should_propagate_second_failure()
    {
        var resultTask = Task.FromResult(Result<int>.Success(5));

        var bound = await resultTask.BindAsync(x => Task.FromResult(Result<string>.Failure("second error")));

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("second error");
    }

    [Fact]
    public async Task BindAsync_should_throw_when_func_is_null()
    {
        var resultTask = Task.FromResult(Result<int>.Success(5));

        var act = async () => await resultTask.BindAsync<int, string>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region EnsureAsync

    [Fact]
    public async Task EnsureAsync_should_pass_when_predicate_is_true()
    {
        var resultTask = Task.FromResult(Result<int>.Success(10));

        var ensured = await resultTask.EnsureAsync(x => Task.FromResult(x > 5), "value too small");

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(10);
    }

    [Fact]
    public async Task EnsureAsync_should_fail_when_predicate_is_false()
    {
        var resultTask = Task.FromResult(Result<int>.Success(3));

        var ensured = await resultTask.EnsureAsync(x => Task.FromResult(x > 5), "value too small");

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().Be("value too small");
    }

    [Fact]
    public async Task EnsureAsync_should_propagate_failure()
    {
        var resultTask = Task.FromResult(Result<int>.Failure("original error"));

        var ensured = await resultTask.EnsureAsync(x => Task.FromResult(x > 5), "value too small");

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().Be("original error");
    }

    [Fact]
    public async Task EnsureAsync_should_throw_when_predicate_is_null()
    {
        var resultTask = Task.FromResult(Result<int>.Success(10));

        var act = async () => await resultTask.EnsureAsync(null!, "error");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region TapAsync

    [Fact]
    public async Task TapAsync_should_execute_action_on_success()
    {
        var resultTask = Task.FromResult(Result<int>.Success(10));
        var executed = false;

        var tapped = await resultTask.TapAsync(x =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        executed.Should().BeTrue();
        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(10);
    }

    [Fact]
    public async Task TapAsync_should_not_execute_action_on_failure()
    {
        var resultTask = Task.FromResult(Result<int>.Failure("error"));
        var executed = false;

        var tapped = await resultTask.TapAsync(x =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        executed.Should().BeFalse();
        tapped.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_should_throw_when_action_is_null()
    {
        var resultTask = Task.FromResult(Result<int>.Success(10));

        var act = async () => await resultTask.TapAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region TapFailureAsync (Task<Result<T>> → Task<Result<T>>)

    [Fact]
    public async Task TapFailureAsync_should_execute_action_on_failure()
    {
        var resultTask = Task.FromResult(Result<int>.Failure("test error"));
        var capturedError = string.Empty;

        var tapped = await resultTask.TapFailureAsync(error =>
        {
            capturedError = error;
            return Task.CompletedTask;
        });

        capturedError.Should().Be("test error");
        tapped.IsFailure.Should().BeTrue();
        tapped.Error.Should().Be("test error");
    }

    [Fact]
    public async Task TapFailureAsync_should_not_execute_action_on_success()
    {
        var resultTask = Task.FromResult(Result<int>.Success(10));
        var executed = false;

        var tapped = await resultTask.TapFailureAsync(error =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        executed.Should().BeFalse();
        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(10);
    }

    [Fact]
    public async Task TapFailureAsync_should_throw_when_action_is_null()
    {
        var resultTask = Task.FromResult(Result<int>.Failure("error"));

        var act = async () => await resultTask.TapFailureAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region MatchAsync

    [Fact]
    public async Task MatchAsync_should_call_onSuccess_when_success()
    {
        var resultTask = Task.FromResult(Result<int>.Success(10));

        var matched = await resultTask.MatchAsync(
            onSuccess: x => Task.FromResult($"Success: {x}"),
            onFailure: e => Task.FromResult($"Error: {e}")
        );

        matched.Should().Be("Success: 10");
    }

    [Fact]
    public async Task MatchAsync_should_call_onFailure_when_failure()
    {
        var resultTask = Task.FromResult(Result<int>.Failure("test error"));

        var matched = await resultTask.MatchAsync(
            onSuccess: x => Task.FromResult($"Success: {x}"),
            onFailure: e => Task.FromResult($"Error: {e}")
        );

        matched.Should().Be("Error: test error");
    }

    [Fact]
    public async Task MatchAsync_should_throw_when_onSuccess_is_null()
    {
        var resultTask = Task.FromResult(Result<int>.Success(10));

        var act = async () => await resultTask.MatchAsync<int, string>(null!, e => Task.FromResult(e));

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task MatchAsync_should_throw_when_onFailure_is_null()
    {
        var resultTask = Task.FromResult(Result<int>.Success(10));

        var act = async () => await resultTask.MatchAsync<int, string>(x => Task.FromResult(x.ToString()), null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region MapAsync (Result<T> → Task<Result<U>>)

    [Fact]
    public async Task MapAsync_sync_should_transform_success()
    {
        var result = Result<int>.Success(10);

        var mapped = await result.MapAsync(x => Task.FromResult(x * 2));

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(20);
    }

    [Fact]
    public async Task MapAsync_sync_should_propagate_failure()
    {
        var result = Result<int>.Failure("test error");

        var mapped = await result.MapAsync(x => Task.FromResult(x * 2));

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be("test error");
    }

    [Fact]
    public async Task MapAsync_sync_should_throw_when_result_is_null()
    {
        Result<int> result = null!;

        var act = async () => await result.MapAsync(x => Task.FromResult(x * 2));

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task MapAsync_sync_should_throw_when_func_is_null()
    {
        var result = Result<int>.Success(10);

        var act = async () => await result.MapAsync<int, int>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region BindAsync (Result<T> → Task<Result<U>>)

    [Fact]
    public async Task BindAsync_sync_should_chain_operations()
    {
        var result = Result<int>.Success(10);

        var bound = await result.BindAsync(x => Task.FromResult(Result<string>.Success(x.ToString())));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("10");
    }

    [Fact]
    public async Task BindAsync_sync_should_propagate_first_failure()
    {
        var result = Result<int>.Failure("first error");

        var bound = await result.BindAsync(x => Task.FromResult(Result<string>.Success(x.ToString())));

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("first error");
    }

    [Fact]
    public async Task BindAsync_sync_should_propagate_second_failure()
    {
        var result = Result<int>.Success(10);

        var bound = await result.BindAsync(x => Task.FromResult(Result<string>.Failure("second error")));

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("second error");
    }

    [Fact]
    public async Task BindAsync_sync_should_throw_when_result_is_null()
    {
        Result<int> result = null!;

        var act = async () => await result.BindAsync(x => Task.FromResult(Result<string>.Success(x.ToString())));

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task BindAsync_sync_should_throw_when_func_is_null()
    {
        var result = Result<int>.Success(10);

        var act = async () => await result.BindAsync<int, string>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region EnsureAsync (Result<T> → Task<Result<T>>)

    [Fact]
    public async Task EnsureAsync_sync_should_pass_when_predicate_is_true()
    {
        var result = Result<int>.Success(10);

        var ensured = await result.EnsureAsync(x => Task.FromResult(x > 5), "Value too small");

        ensured.IsSuccess.Should().BeTrue();
        ensured.Value.Should().Be(10);
    }

    [Fact]
    public async Task EnsureAsync_sync_should_fail_when_predicate_is_false()
    {
        var result = Result<int>.Success(3);

        var ensured = await result.EnsureAsync(x => Task.FromResult(x > 5), "Value too small");

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().Be("Value too small");
    }

    [Fact]
    public async Task EnsureAsync_sync_should_propagate_failure()
    {
        var result = Result<int>.Failure("test error");

        var ensured = await result.EnsureAsync(x => Task.FromResult(x > 5), "Value too small");

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().Be("test error");
    }

    [Fact]
    public async Task EnsureAsync_sync_should_throw_when_result_is_null()
    {
        Result<int> result = null!;

        var act = async () => await result.EnsureAsync(x => Task.FromResult(x > 5), "error");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EnsureAsync_sync_should_throw_when_predicate_is_null()
    {
        var result = Result<int>.Success(10);

        var act = async () => await result.EnsureAsync(null!, "error");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region TapAsync (Result<T> → Task<Result<T>>)

    [Fact]
    public async Task TapAsync_sync_should_execute_action_on_success()
    {
        var result = Result<int>.Success(10);
        var sideEffect = 0;

        var tapped = await result.TapAsync(x => Task.Run(() => sideEffect = x * 2));

        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(10);
        sideEffect.Should().Be(20);
    }

    [Fact]
    public async Task TapAsync_sync_should_not_execute_action_on_failure()
    {
        var result = Result<int>.Failure("test error");
        var sideEffect = 0;

        var tapped = await result.TapAsync(x => Task.Run(() => sideEffect = x * 2));

        tapped.IsFailure.Should().BeTrue();
        tapped.Error.Should().Be("test error");
        sideEffect.Should().Be(0);
    }

    [Fact]
    public async Task TapAsync_sync_should_throw_when_result_is_null()
    {
        Result<int> result = null!;

        var act = async () => await result.TapAsync(x => Task.CompletedTask);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task TapAsync_sync_should_throw_when_action_is_null()
    {
        var result = Result<int>.Success(10);

        var act = async () => await result.TapAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region TapFailureAsync (Result<T> → Task<Result<T>>)

    [Fact]
    public async Task TapFailureAsync_sync_should_execute_action_on_failure()
    {
        var result = Result<int>.Failure("test error");
        var capturedError = string.Empty;

        var tapped = await result.TapFailureAsync(error => Task.Run(() => capturedError = error));

        tapped.IsFailure.Should().BeTrue();
        tapped.Error.Should().Be("test error");
        capturedError.Should().Be("test error");
    }

    [Fact]
    public async Task TapFailureAsync_sync_should_not_execute_action_on_success()
    {
        var result = Result<int>.Success(10);
        var executed = false;

        var tapped = await result.TapFailureAsync(error => Task.Run(() => executed = true));

        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(10);
        executed.Should().BeFalse();
    }

    [Fact]
    public async Task TapFailureAsync_sync_should_throw_when_result_is_null()
    {
        Result<int> result = null!;

        var act = async () => await result.TapFailureAsync(error => Task.CompletedTask);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task TapFailureAsync_sync_should_throw_when_action_is_null()
    {
        var result = Result<int>.Failure("error");

        var act = async () => await result.TapFailureAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region ToResult

    [Fact]
    public void ToResult_should_convert_success_result_to_non_generic()
    {
        var result = Result<int>.Success(42);

        var converted = result.ToResult();

        converted.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToResult_should_convert_failure_result_to_non_generic()
    {
        var result = Result<string>.Failure("test error");

        var converted = result.ToResult();

        converted.IsFailure.Should().BeTrue();
        converted.Error.Should().Be("test error");
    }

    [Fact]
    public void ToResult_should_throw_when_result_is_null()
    {
        Result<int> result = null!;

        var act = () => result.ToResult();

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ToResult (Result → Result<T>)

    [Fact]
    public void ToResult_with_value_should_convert_success_result()
    {
        var result = Result.Success();

        var converted = result.ToResult(42);

        converted.IsSuccess.Should().BeTrue();
        converted.Value.Should().Be(42);
    }

    [Fact]
    public void ToResult_with_value_should_convert_failure_result()
    {
        var result = Result.Failure("test error");

        var converted = result.ToResult(42);

        converted.IsFailure.Should().BeTrue();
        converted.Error.Should().Be("test error");
    }

    [Fact]
    public void ToResult_with_value_should_throw_when_result_is_null()
    {
        Result result = null!;

        var act = () => result.ToResult(42);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToResult_with_value_should_work_with_complex_types()
    {
        var result = Result.Success();
        var credentials = (Email: "test@example.com", Password: "password123");

        var converted = result.ToResult(credentials);

        converted.IsSuccess.Should().BeTrue();
        converted.Value.Email.Should().Be("test@example.com");
        converted.Value.Password.Should().Be("password123");
    }

    #endregion

    #region Ensure (Result - stateless)

    [Fact]
    public void Ensure_stateless_should_pass_when_predicate_is_true()
    {
        var result = Result.Success();

        var ensured = result.Ensure(() => true, "test error");

        ensured.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Ensure_stateless_should_fail_when_predicate_is_false()
    {
        var result = Result.Success();

        var ensured = result.Ensure(() => false, "test error");

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().Be("test error");
    }

    [Fact]
    public void Ensure_stateless_should_propagate_failure()
    {
        var result = Result.Failure("original error");

        var ensured = result.Ensure(() => true, "new error");

        ensured.IsFailure.Should().BeTrue();
        ensured.Error.Should().Be("original error");
    }

    [Fact]
    public void Ensure_stateless_should_throw_when_result_is_null()
    {
        Result result = null!;

        var act = () => result.Ensure(() => true, "error");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ensure_stateless_should_throw_when_predicate_is_null()
    {
        var result = Result.Success();

        var act = () => result.Ensure(null!, "error");

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
