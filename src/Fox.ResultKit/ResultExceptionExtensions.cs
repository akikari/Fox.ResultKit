//==================================================================================================
// Extension methods for creating Result from exceptions.
// Exception to Result conversion methods for unified error handling.
//==================================================================================================

namespace Fox.ResultKit;

//==================================================================================================
/// <summary>
/// Extension methods for creating Result from exceptions.
/// </summary>
//==================================================================================================
public static class ResultExceptionExtensions
{
    //==============================================================================================
    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="ex">The exception from which the result is created.</param>
    /// <returns>Failed result with the exception message.</returns>
    //==============================================================================================
    public static Result<T> FromException<T>(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return Result<T>.Failure(GetExceptionMessage(ex, includeInnerExceptions: false, includeStackTrace: false));
    }

    //==============================================================================================
    /// <summary>
    /// Creates a failed result from an exception with detailed information.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="ex">The exception from which the result is created.</param>
    /// <param name="includeInnerExceptions">Whether to include inner exceptions.</param>
    /// <param name="includeStackTrace">Whether to include the stack trace.</param>
    /// <returns>Failed result with detailed exception information.</returns>
    //==============================================================================================
    public static Result<T> FromException<T>(Exception ex, bool includeInnerExceptions, bool includeStackTrace)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return Result<T>.Failure(GetExceptionMessage(ex, includeInnerExceptions, includeStackTrace));
    }

    //==============================================================================================
    /// <summary>
    /// Constructs an error message from an exception based on the specified parameters.
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <param name="includeInnerExceptions">Whether to include inner exceptions.</param>
    /// <param name="includeStackTrace">Whether to include the stack trace.</param>
    /// <returns>The constructed error message.</returns>
    //==============================================================================================
    private static string GetExceptionMessage(Exception ex, bool includeInnerExceptions, bool includeStackTrace)
    {
        List<string> parts = [];

        if (ex is AggregateException aggEx)
        {
            var messages = aggEx.Flatten().InnerExceptions.Select(e => e.Message);
            parts.Add(string.Join("; ", messages));
        }
        else
        {
            parts.Add(ex.Message);
        }

        if (includeInnerExceptions && ex.InnerException != null)
        {
            var current = ex.InnerException;
            while (current != null)
            {
                parts.Add($"Inner: {current.Message}");
                current = current.InnerException;
            }
        }

        if (includeStackTrace && ex.StackTrace != null)
        {
            parts.Add($"Stack:{Environment.NewLine}{ex.StackTrace}");
        }

        return string.Join(Environment.NewLine, parts);
    }
}
