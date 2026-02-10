//==================================================================================================
// Result extension methods for exception handling.
// Try methods for converting exception-based operations to Result pattern.
//==================================================================================================

namespace Fox.ResultKit;

//==================================================================================================
/// <summary>
/// Result extension methods for exception handling.
/// </summary>
//==================================================================================================
public static class ResultTryExtensions
{
    //==============================================================================================
    /// <summary>
    /// Executes an operation and returns a successful result, or a failed result on exception.
    /// </summary>
    /// <typeparam name="T">The operation return value type.</typeparam>
    /// <param name="func">The operation to execute.</param>
    /// <param name="error">The error message on exception.</param>
    /// <returns>Successful result with the operation value, or failed result on exception.</returns>
    //==============================================================================================
    public static Result<T> Try<T>(Func<T> func, string error)
    {
        ArgumentNullException.ThrowIfNull(func);

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            return Result<T>.Success(func());
        }
        catch
        {
            return Result<T>.Failure(error);
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }

    //==============================================================================================
    /// <summary>
    /// Asynchronously executes an operation and returns a successful result, or a failed result on exception.
    /// </summary>
    /// <typeparam name="T">The operation return value type.</typeparam>
    /// <param name="func">The asynchronous operation to execute.</param>
    /// <param name="error">The error message on exception.</param>
    /// <returns>Successful result with the operation value, or failed result on exception.</returns>
    //==============================================================================================
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> func, string error)
    {
        ArgumentNullException.ThrowIfNull(func);

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            T value = await func();
            return Result<T>.Success(value);
        }
        catch
        {
            return Result<T>.Failure(error);
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
