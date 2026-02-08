//==================================================================================================
// Functional extension methods for Result types.
// Map, Bind, Tap, Ensure, Match, ToResult methods for ROP and null handling support.
//==================================================================================================

namespace Fox.ResultKit;

//==================================================================================================
/// <summary>
/// Functional extension methods for Result types.
/// </summary>
//==================================================================================================
public static class ResultExtensions
{
    #region Szinkron bővítmények

    //==============================================================================================
    /// <summary>
    /// Maps the result value to a new type using the specified function.
    /// </summary>
    /// <typeparam name="T">The source result value type.</typeparam>
    /// <typeparam name="U">The target result value type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="func">The mapping function.</param>
    /// <returns>The transformed value on success, or the original error on failure.</returns>
    /// <remarks>
    /// If the mapping function throws an exception, the exception propagates and breaks the chain.
    /// For exception-safe execution, use the <see cref="ResultTryExtensions.Try{T}"/> method
    /// combined with the <see cref="Bind{T, U}"/> method.
    /// </remarks>
    //==============================================================================================
    public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> func)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(func);

        if (result.IsFailure)
        {
            return Result<U>.Failure(result.Error!);
        }

        return Result<U>.Success(func(result.Value!));
    }

    //==============================================================================================
    /// <summary>
    /// Chains another operation to the result, which itself returns a Result.
    /// </summary>
    /// <typeparam name="T">The source result value type.</typeparam>
    /// <typeparam name="U">The target result value type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="func">The chained operation function.</param>
    /// <returns>The result of the chained operation or the original error.</returns>
    /// <remarks>
    /// If the chained operation function throws an exception, the exception propagates and breaks
    /// the chain. For exception-safe execution, use the <see cref="ResultTryExtensions.Try{T}"/>
    /// method inside the function.
    /// </remarks>
    //==============================================================================================
    public static Result<U> Bind<T, U>(this Result<T> result, Func<T, Result<U>> func)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(func);

        if (result.IsFailure)
        {
            return Result<U>.Failure(result.Error!);
        }

        return func(result.Value!);
    }

    //==============================================================================================
    /// <summary>
    /// Verifies that the result satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="predicate">The verification condition.</param>
    /// <param name="error">The error message if the condition is not met.</param>
    /// <returns>The original result if the condition is met, otherwise a failed result.</returns>
    /// <remarks>
    /// If the verification condition throws an exception, the exception propagates and breaks the chain.
    /// </remarks>
    //==============================================================================================
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, string error)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(predicate);

        if (result.IsFailure)
        {
            return result;
        }

        if (!predicate(result.Value!))
        {
            return Result<T>.Failure(error);
        }

        return result;
    }

    //==============================================================================================
    /// <summary>
    /// Executes a side-effect operation on successful result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="action">The operation to execute.</param>
    /// <returns>The original result unchanged.</returns>
    /// <remarks>
    /// If the operation throws an exception, the exception propagates and breaks the chain.
    /// </remarks>
    //==============================================================================================
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
        {
            action(result.Value!);
        }

        return result;
    }

    //==============================================================================================
    /// <summary>
    /// Executes a side-effect operation on failed result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="action">The operation to execute with the error message.</param>
    /// <returns>The original result unchanged.</returns>
    /// <remarks>
    /// If the operation throws an exception, the exception propagates and breaks the chain.
    /// This method is useful for logging or other side effects on failure.
    /// </remarks>
    //==============================================================================================
    public static Result<T> TapFailure<T>(this Result<T> result, Action<string> action)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsFailure)
        {
            action(result.Error!);
        }

        return result;
    }

    //==============================================================================================
    /// <summary>
    /// Performs a pattern matching operation on the result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <typeparam name="U">The return value type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="onSuccess">Function to execute on successful result.</param>
    /// <param name="onFailure">Function to execute on failed result.</param>
    /// <returns>The result of the matching branch.</returns>
    /// <remarks>
    /// If either matching function throws an exception, the exception propagates.
    /// </remarks>
    //==============================================================================================
    public static U Match<T, U>(this Result<T> result, Func<T, U> onSuccess, Func<string, U> onFailure)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        if (result.IsSuccess)
        {
            return onSuccess(result.Value!);
        }

        return onFailure(result.Error!);
    }

    //==============================================================================================
    /// <summary>
    /// Verifies that a condition is met.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="predicate">The verification condition.</param>
    /// <param name="error">The error message if the condition is not met.</param>
    /// <returns>The original result if the condition is met, otherwise a failed result.</returns>
    /// <remarks>
    /// If the verification condition throws an exception, the exception propagates and breaks the chain.
    /// This method is useful for stateless validation where the condition does not depend on the
    /// result value.
    /// </remarks>
    //==============================================================================================
    public static Result Ensure(this Result result, Func<bool> predicate, string error)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(predicate);

        if (result.IsFailure)
        {
            return result;
        }

        return predicate() ? result : Result.Failure(error);
    }

    //==============================================================================================
    /// <summary>
    /// Converts the generic result to a non-generic result.
    /// </summary>
    /// <typeparam name="T">The original result value type.</typeparam>
    /// <param name="result">The generic result.</param>
    /// <returns>Non-generic result with the original status and error message.</returns>
    /// <remarks>
    /// The result value is lost during conversion; only the success/failure status and
    /// error message are preserved.
    /// </remarks>
    //==============================================================================================
    public static Result ToResult<T>(this Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess ? Result.Success() : Result.Failure(result.Error!);
    }

    //==============================================================================================
    /// <summary>
    /// Converts the non-generic result to a generic result with a specified value.
    /// </summary>
    /// <typeparam name="T">The target result value type.</typeparam>
    /// <param name="result">The non-generic result.</param>
    /// <param name="value">The value to assign to the successful result.</param>
    /// <returns>Successful result with the value, or failed result with the original error message.</returns>
    /// <remarks>
    /// This method is useful in validation pipelines where you first check (Result),
    /// then add the value for continuation (Result{T}).
    /// </remarks>
    //==============================================================================================
    public static Result<T> ToResult<T>(this Result result, T value)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.IsSuccess ? Result<T>.Success(value) : Result<T>.Failure(result.Error!);
    }

    //==============================================================================================
    /// <summary>
    /// Converts a nullable value to a Result type.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value to convert.</param>
    /// <param name="errorIfNull">The error message if the value is null.</param>
    /// <returns>Successful result with the value, or failed result if the value is null.</returns>
    /// <remarks>
    /// This method is useful for integrating null handling into the Result pattern, especially
    /// at the start of pipelines where you begin with a nullable source.
    /// </remarks>
    //==============================================================================================
    public static Result<T> ToResult<T>(this T? value, string errorIfNull) where T : class
    {
        if (value == null)
        {
            return Result<T>.Failure(errorIfNull);
        }

        return Result<T>.Success(value);
    }

    #endregion

    #region Async extensions (Result<T> → Task<Result<U>>)

    //==============================================================================================
    /// <summary>
    /// Asynchronously maps the result value to a new type.
    /// </summary>
    /// <typeparam name="T">The source result value type.</typeparam>
    /// <typeparam name="U">The target result value type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="func">The asynchronous mapping function.</param>
    /// <returns>The transformed value on success, or the original error on failure.</returns>
    /// <remarks>
    /// If the asynchronous mapping function throws an exception, the exception propagates and breaks
    /// the chain. For exception-safe execution, use the <see cref="ResultTryExtensions.TryAsync{T}"/>
    /// method combined with the <see cref="BindAsync{T, U}(Result{T}, Func{T, Task{Result{U}}})"/> method.
    /// </remarks>
    //==============================================================================================
    public static async Task<Result<U>> MapAsync<T, U>(this Result<T> result, Func<T, Task<U>> func)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(func);

        if (result.IsFailure)
        {
            return Result<U>.Failure(result.Error!);
        }

        U value = await func(result.Value!);
        return Result<U>.Success(value);
    }

    //==============================================================================================
    /// <summary>
    /// Asynchronously chains another operation to the result.
    /// </summary>
    /// <typeparam name="T">The source result value type.</typeparam>
    /// <typeparam name="U">The target result value type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="func">The asynchronous chained operation function.</param>
    /// <returns>The result of the chained operation or the original error.</returns>
    /// <remarks>
    /// If the asynchronous chained operation function throws an exception, the exception propagates
    /// and breaks the chain. For exception-safe execution, use the
    /// <see cref="ResultTryExtensions.TryAsync{T}"/> method inside the function.
    /// </remarks>
    //==============================================================================================
    public static async Task<Result<U>> BindAsync<T, U>(this Result<T> result, Func<T, Task<Result<U>>> func)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(func);

        if (result.IsFailure)
        {
            return Result<U>.Failure(result.Error!);
        }

        return await func(result.Value!);
    }

    //==============================================================================================
    /// <summary>
    /// Asynchronously verifies that the result satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="predicate">The asynchronous verification condition.</param>
    /// <param name="error">The error message if the condition is not met.</param>
    /// <returns>The original result if the condition is met, otherwise a failed result.</returns>
    /// <remarks>
    /// If the asynchronous verification condition throws an exception, the exception propagates
    /// and breaks the chain.
    /// </remarks>
    //==============================================================================================
    public static async Task<Result<T>> EnsureAsync<T>(this Result<T> result, Func<T, Task<bool>> predicate, string error)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(predicate);

        if (result.IsFailure)
        {
            return result;
        }

        bool isValid = await predicate(result.Value!);

        if (!isValid)
        {
            return Result<T>.Failure(error);
        }

        return result;
    }

    //==============================================================================================
    /// <summary>
    /// Asynchronously executes a side-effect operation on successful result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="action">The asynchronous operation to execute.</param>
    /// <returns>The original result unchanged.</returns>
    /// <remarks>
    /// If the asynchronous operation throws an exception, the exception propagates and breaks the chain.
    /// </remarks>
    //==============================================================================================
    public static async Task<Result<T>> TapAsync<T>(this Result<T> result, Func<T, Task> action)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
        {
            await action(result.Value!);
        }

        return result;
    }

    //==============================================================================================
    /// <summary>
    /// Asynchronously executes a side-effect operation on failed result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="action">The asynchronous operation to execute with the error message.</param>
    /// <returns>The original result unchanged.</returns>
    /// <remarks>
    /// If the asynchronous operation throws an exception, the exception propagates and breaks the chain.
    /// This method is useful for logging or other side effects on failure.
    /// </remarks>
    //==============================================================================================
    public static async Task<Result<T>> TapFailureAsync<T>(this Result<T> result, Func<string, Task> action)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsFailure)
        {
            await action(result.Error!);
        }

        return result;
    }

    #endregion

    #region Async extensions (Task<Result<T>> → Task<Result<U>>)

    //==============================================================================================
    /// <summary>
    /// Asynchronously maps the result value to a new type.
    /// </summary>
    /// <typeparam name="T">The source result value type.</typeparam>
    /// <typeparam name="U">The target result value type.</typeparam>
    /// <param name="resultTask">The task containing the source result.</param>
    /// <param name="func">The asynchronous mapping function.</param>
    /// <returns>The transformed value on success, or the original error on failure.</returns>
    /// <remarks>
    /// If the asynchronous mapping function throws an exception, the exception propagates and breaks
    /// the chain. For exception-safe execution, use the <see cref="ResultTryExtensions.TryAsync{T}"/>
    /// method combined with the <see cref="BindAsync{T, U}(Task{Result{T}}, Func{T, Task{Result{U}}})"/> method.
    /// </remarks>
    //==============================================================================================
    public static async Task<Result<U>> MapAsync<T, U>(this Task<Result<T>> resultTask, Func<T, Task<U>> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        Result<T> result = await resultTask;

        if (result.IsFailure)
        {
            return Result<U>.Failure(result.Error!);
        }

        U value = await func(result.Value!);
        return Result<U>.Success(value);
    }

    //==============================================================================================
    /// <summary>
    /// Asynchronously chains another operation to the result.
    /// </summary>
    /// <typeparam name="T">The source result value type.</typeparam>
    /// <typeparam name="U">The target result value type.</typeparam>
    /// <param name="resultTask">The task containing the source result.</param>
    /// <param name="func">The asynchronous chained operation function.</param>
    /// <returns>The result of the chained operation or the original error.</returns>
    /// <remarks>
    /// If the asynchronous chained operation function throws an exception, the exception propagates
    /// and breaks the chain. For exception-safe execution, use the
    /// <see cref="ResultTryExtensions.TryAsync{T}"/> method inside the function.
    /// </remarks>
    //==============================================================================================
    public static async Task<Result<U>> BindAsync<T, U>(this Task<Result<T>> resultTask, Func<T, Task<Result<U>>> func)
    {
        ArgumentNullException.ThrowIfNull(func);

        Result<T> result = await resultTask;

        if (result.IsFailure)
        {
            return Result<U>.Failure(result.Error!);
        }

        return await func(result.Value!);
    }

    //==============================================================================================
    /// <summary>
    /// Asynchronously verifies that the result satisfies a condition.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="predicate">The asynchronous verification condition.</param>
    /// <param name="error">The error message if the condition is not met.</param>
    /// <returns>The original result if the condition is met, otherwise a failed result.</returns>
    /// <remarks>
    /// If the asynchronous verification condition throws an exception, the exception propagates
    /// and breaks the chain.
    /// </remarks>
    //==============================================================================================
    public static async Task<Result<T>> EnsureAsync<T>(this Task<Result<T>> resultTask, Func<T, Task<bool>> predicate, string error)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        Result<T> result = await resultTask;

        if (result.IsFailure)
        {
            return result;
        }

        bool isValid = await predicate(result.Value!);

        if (!isValid)
        {
            return Result<T>.Failure(error);
        }

        return result;
    }

    //==============================================================================================
    /// <summary>
    /// Asynchronously executes a side-effect operation on successful result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="action">The asynchronous operation to execute.</param>
    /// <returns>The original result unchanged.</returns>
    /// <remarks>
    /// If the asynchronous operation throws an exception, the exception propagates and breaks the chain.
    /// </remarks>
    //==============================================================================================
    public static async Task<Result<T>> TapAsync<T>(this Task<Result<T>> resultTask, Func<T, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        Result<T> result = await resultTask;

        if (result.IsSuccess)
        {
            await action(result.Value!);
        }

        return result;
    }

    //==============================================================================================
    /// <summary>
    /// Asynchronously executes a side-effect operation on failed result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="action">The asynchronous operation to execute with the error message.</param>
    /// <returns>The original result unchanged.</returns>
    /// <remarks>
    /// If the asynchronous operation throws an exception, the exception propagates and breaks the chain.
    /// This method is useful for logging or other side effects on failure.
    /// </remarks>
    //==============================================================================================
    public static async Task<Result<T>> TapFailureAsync<T>(this Task<Result<T>> resultTask, Func<string, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        Result<T> result = await resultTask;

        if (result.IsFailure)
        {
            await action(result.Error!);
        }

        return result;
    }

    //==============================================================================================
    /// <summary>
    /// Asynchronously performs a pattern matching operation on the result.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <typeparam name="U">The return value type.</typeparam>
    /// <param name="resultTask">The task containing the result.</param>
    /// <param name="onSuccess">Asynchronous function to execute on successful result.</param>
    /// <param name="onFailure">Asynchronous function to execute on failed result.</param>
    /// <returns>The result of the matching branch.</returns>
    /// <remarks>
    /// If either asynchronous matching function throws an exception, the exception propagates.
    /// </remarks>
    //==============================================================================================
    public static async Task<U> MatchAsync<T, U>(this Task<Result<T>> resultTask, Func<T, Task<U>> onSuccess, Func<string, Task<U>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);

        Result<T> result = await resultTask;

        if (result.IsSuccess)
        {
            return await onSuccess(result.Value!);
        }

        return await onFailure(result.Error!);
    }

    #endregion
}
