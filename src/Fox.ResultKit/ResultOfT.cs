//==================================================================================================
// Represents the result of an operation with generic value.
// Type-safe Result pattern sealed class implementation.
//==================================================================================================

namespace Fox.ResultKit;

//==================================================================================================
/// <summary>
/// Represents the result of an operation.
/// </summary>
/// <typeparam name="T">Type of the success value.</typeparam>
//==================================================================================================
public sealed class Result<T>
{
    #region Fields

    private readonly T? value;

    #endregion

    #region Constructors

    //==============================================================================================
    /// <summary>
    /// Creates a Result&lt;T&gt; instance.
    /// </summary>
    /// <param name="isSuccess">True if operation succeeded, false otherwise.</param>
    /// <param name="value">Operation value on success.</param>
    /// <param name="error">Error message for failed operation, null otherwise.</param>
    //==============================================================================================
    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        this.value = value;
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
    public bool IsFailure => !IsSuccess;

    //==============================================================================================
    /// <summary>
    /// Returns the successful operation value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// If <see cref="IsFailure"/> is true.
    /// </exception>
    //==============================================================================================
    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException(Error);
            }

            return value!;
        }
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
    /// Creates a successful result with specified value.
    /// </summary>
    /// <param name="value">Successful operation result value.</param>
    /// <returns><see cref="Result{T}"/> instance representing successful operation.</returns>
    //==============================================================================================
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    //==============================================================================================
    /// <summary>
    /// Creates a failed result with specified error message.
    /// </summary>
    /// <param name="error">Error message describing the failure reason.</param>
    /// <returns><see cref="Result{T}"/> instance representing failed operation.</returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="error"/> is null or empty.
    /// </exception>
    //==============================================================================================
    public static Result<T> Failure(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        return new Result<T>(false, default, error);
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
