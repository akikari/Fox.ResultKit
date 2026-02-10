//==================================================================================================
// Common interface for Result and Result<T> enabling polymorphic error collection.
// Abstraction for Railway Oriented Programming result types supporting mixed-type scenarios.
//==================================================================================================
using System.Diagnostics.CodeAnalysis;

namespace Fox.ResultKit;

//==================================================================================================
/// <summary>
/// Represents the common contract for result types in Railway Oriented Programming.
/// </summary>
/// <remarks>
/// This interface enables collecting errors from mixed Result and Result&lt;T&gt; types,
/// supporting scenarios like validation aggregation, batch processing, and custom result
/// implementations. Both <see cref="Result"/> and <see cref="Result{T}"/> implement this interface.
/// </remarks>
//==================================================================================================
public interface IResult
{
    //==============================================================================================
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    //==============================================================================================
    bool IsSuccess { get; }

    //==============================================================================================
    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    //==============================================================================================
    bool IsFailure { get; }

    //==============================================================================================
    /// <summary>
    /// Gets the error message if the operation failed, or null if successful.
    /// </summary>
    //==============================================================================================
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is the established API convention for Result types since v1.0.0")]
    string? Error { get; }
}
