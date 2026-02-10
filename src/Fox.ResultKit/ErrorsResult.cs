//==================================================================================================
// Represents the collected result of multiple operations with aggregated errors.
// Record struct for gathering multiple Result outcomes preserving all failure messages.
//==================================================================================================

namespace Fox.ResultKit;

//==================================================================================================
/// <summary>
/// Represents the collected result of multiple operations with aggregated errors.
/// </summary>
/// <param name="IsSuccess">True if all operations succeeded, false otherwise.</param>
/// <param name="Errors">Collection of error messages from failed operations.</param>
/// <remarks>
/// Use this type when you need to execute multiple operations and collect all errors,
/// rather than failing fast on the first error. Common scenarios include form validation,
/// batch processing, and multi-step workflows where showing all errors at once improves UX.
/// </remarks>
//==================================================================================================
public readonly record struct ErrorsResult(bool IsSuccess, IReadOnlyList<string> Errors)
{
    #region Properties

    //==============================================================================================
    /// <summary>
    /// Indicates whether any operation failed.
    /// </summary>
    //==============================================================================================
    public bool IsFailure => !IsSuccess;

    //==============================================================================================
    /// <summary>
    /// Creates a successful result with no errors.
    /// </summary>
    /// <returns>Successful <see cref="ErrorsResult"/> instance.</returns>
    //==============================================================================================
    public static ErrorsResult Success() => new(true, []);

    #endregion

    #region Public methods

    //==============================================================================================
    /// <summary>
    /// Collects multiple results into a single result with aggregated errors.
    /// </summary>
    /// <param name="results">The results to collect (supports mixed Result and Result&lt;T&gt; types).</param>
    /// <returns>Successful result if all inputs succeeded, otherwise result with all errors.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="results"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Mixed Result and Result&lt;T&gt; types supported
    /// var validation = ErrorsResult.Collect(
    ///     ValidateEmail(email),        // Result
    ///     ValidatePassword(password),  // Result
    ///     ParseAge(ageInput)          // Result&lt;int&gt;
    /// );
    /// 
    /// if (validation.IsFailure)
    /// {
    ///     foreach (var error in validation.Errors)
    ///     {
    ///         Console.WriteLine(error);
    ///     }
    /// }
    /// </code>
    /// </example>
    //==============================================================================================
    public static ErrorsResult Collect(params IResult[] results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var errors = results
            .Where(r => r.IsFailure)
            .Select(r => r.Error!)
            .ToList();

        return new ErrorsResult(errors.Count == 0, errors);
    }

    //==============================================================================================
    /// <summary>
    /// Converts this ErrorsResult to a Result by combining all errors into a single message.
    /// </summary>
    /// <returns>Successful Result if no errors, otherwise Failure with combined error message.</returns>
    /// <remarks>
    /// Errors are joined with newlines. Use this when you need to integrate ErrorsResult
    /// into a Result-based pipeline, though separation is generally recommended.
    /// </remarks>
    //==============================================================================================
    public Result ToResult()
    {
        return IsSuccess
            ? Result.Success()
            : Result.Failure(string.Join(Environment.NewLine, Errors));
    }

    #endregion
}
