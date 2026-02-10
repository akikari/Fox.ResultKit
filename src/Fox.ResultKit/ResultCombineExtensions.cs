//==================================================================================================
// Extension methods for combining multiple results.
// Combine methods for aggregating multiple validation Results.
//==================================================================================================

namespace Fox.ResultKit;

//==================================================================================================
/// <summary>
/// Extension methods for combining multiple results.
/// </summary>
//==================================================================================================
public static class ResultCombineExtensions
{
    //==============================================================================================
    /// <summary>
    /// Combines multiple results into a single result.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>Successful result if all input results are successful, otherwise the first
    /// result containing an error.</returns>
    //==============================================================================================
    public static Result Combine(params Result[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        foreach (Result result in results)
        {
            if (result.IsFailure)
            {
                return Result.Failure(result.Error!);
            }
        }

        return Result.Success();
    }

    //==============================================================================================
    /// <summary>
    /// Combines multiple results into a single generic result with a specified value.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="value">The successful result value.</param>
    /// <param name="results">The results to combine.</param>
    /// <returns>Successful result with the specified value if all input results are successful,
    /// otherwise the first result containing an error.</returns>
    //==============================================================================================
    public static Result<T> Combine<T>(T value, params Result[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        foreach (Result result in results)
        {
            if (result.IsFailure)
            {
                return Result<T>.Failure(result.Error!);
            }
        }

        return Result<T>.Success(value);
    }

    //==============================================================================================
    /// <summary>
    /// Combines multiple generic results into a single result with the last successful result's
    /// value.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="results">The results to combine (at least one element required).</param>
    /// <returns>Successful result with the last element's value if all input results are successful,
    /// otherwise the first result containing an error.</returns>
    /// <exception cref="ArgumentException">If the results array is empty.</exception>
    //==============================================================================================
    public static Result<T> Combine<T>(params Result<T>[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        if (results.Length == 0)
        {
            throw new ArgumentException("At least one result is required.", nameof(results));
        }

        foreach (Result<T> result in results)
        {
            if (result.IsFailure)
            {
                return Result<T>.Failure(result.Error!);
            }
        }

        return Result<T>.Success(results[^1].Value);
    }
}
