//==================================================================================================
// Represents the result of an operation.
// Non-generic Result pattern sealed class implementation for operations without value.
//==================================================================================================

namespace Fox.ResultKit;

//==================================================================================================
/// <summary>
/// Represents the result of an operation.
/// </summary>
//==================================================================================================
public sealed class Result
{
    #region Constructors

    //==============================================================================================
    /// <summary>
    /// Creates a Result instance.
    /// </summary>
    /// <param name="isSuccess">True if operation succeeded, false otherwise.</param>
    /// <param name="error">Error message for failed operation, null otherwise.</param>
    //==============================================================================================
    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    #endregion

    #region Properties

    //==============================================================================================
    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    //==============================================================================================
    public bool IsSuccess { get; }

    //==============================================================================================
    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    //==============================================================================================
    public bool IsFailure
    {
        get { return !IsSuccess; }
    }

    //==============================================================================================
    /// <summary>
    /// Returns error message if operation failed, null otherwise.
    /// </summary>
    //==============================================================================================
    public string? Error { get; }

    #endregion

    #region Public methods

    //==============================================================================================
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns><see cref="Result"/> instance representing successful operation.</returns>
    //==============================================================================================
    public static Result Success()
    {
        return new Result(true, null);
    }

    //==============================================================================================
    /// <summary>
    /// Creates a failed result with specified error message.
    /// </summary>
    /// <param name="error">Error message describing the failure reason.</param>
    /// <returns><see cref="Result"/> instance representing failed operation.</returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="error"/> is null or empty.
    /// </exception>
    //==============================================================================================
    public static Result Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new Result(false, error);
    }

    //==============================================================================================
    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if operation failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// If <see cref="IsFailure"/> is true.
    /// </exception>
    //==============================================================================================
    public void ThrowIfFailure()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException(Error);
        }
    }

    #endregion
}
