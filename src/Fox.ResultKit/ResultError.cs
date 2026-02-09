//==================================================================================================
// Provides utilities for working with structured error messages using conventions.
// Convention-based approach to embed error codes in string-based errors.
//==================================================================================================

namespace Fox.ResultKit;

//==================================================================================================
/// <summary>
/// Provides utilities for working with structured error messages using conventions.
/// </summary>
/// <remarks>
/// Fox.ResultKit uses simple string-based errors for lightweight design.
/// This utility demonstrates a convention-based approach to embed error codes.
/// Convention format: "ERROR_CODE: Error message"
/// Example: "USER_EMAIL_EXISTS: Email already exists"
/// </remarks>
//==================================================================================================
public static class ResultError
{
    #region Constants

    private const char Separator = ':';

    #endregion

    #region Public methods

    //==============================================================================================
    /// <summary>
    /// Parses structured error string into code and message components.
    /// </summary>
    /// <param name="error">Error string (format: "CODE: message" or plain message).</param>
    /// <returns>Tuple of (Code, Message). If no code found, Code is empty string.</returns>
    /// <example>
    /// <code>
    /// var (code, message) = ResultError.Parse("USER_NOT_FOUND: User does not exist");
    /// // code = "USER_NOT_FOUND", message = "User does not exist"
    /// 
    /// var (code2, message2) = ResultError.Parse("Simple error message");
    /// // code2 = "", message2 = "Simple error message"
    /// </code>
    /// </example>
    //==============================================================================================
    public static (string Code, string Message) Parse(string error)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(error);

        var separatorIndex = error.IndexOf(Separator);
        if (separatorIndex > 0 && separatorIndex < error.Length - 1)
        {
            var code = error[..separatorIndex].Trim();
            var message = error[(separatorIndex + 1)..].Trim();
            return (code, message);
        }

        return (string.Empty, error);
    }

    //==============================================================================================
    /// <summary>
    /// Creates structured error string with code and message.
    /// </summary>
    /// <param name="code">Error code (e.g., "USER_EMAIL_EXISTS").</param>
    /// <param name="message">Human-readable error message.</param>
    /// <returns>Formatted error string in "CODE: message" format.</returns>
    /// <example>
    /// <code>
    /// var error = ResultError.Create("USER_NOT_FOUND", "User does not exist");
    /// // error = "USER_NOT_FOUND: User does not exist"
    /// 
    /// Result.Failure(ResultError.Create("VALIDATION_EMAIL", "Invalid email format"));
    /// </code>
    /// </example>
    //==============================================================================================
    public static string Create(string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return $"{code}{Separator} {message}";
    }

    #endregion
}
