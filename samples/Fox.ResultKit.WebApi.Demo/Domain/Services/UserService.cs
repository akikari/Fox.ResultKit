//==================================================================================================
// User service demonstrating ResultKit basic usage and functional pipelines.
// Service layer implementation with Railway Oriented Programming patterns.
//==================================================================================================
using Fox.ResultKit.WebApi.Demo.Domain.Models;
using Fox.ResultKit.WebApi.Demo.Domain.Repositories;
using Fox.ResultKit.WebApi.Demo.DTOs;

namespace Fox.ResultKit.WebApi.Demo.Domain.Services;

//==================================================================================================
/// <summary>
/// User service demonstrating ResultKit usage and functional pipelines.
/// </summary>
/// <param name="repository">Repository for user data access.</param>
/// <param name="logger">Logger instance for diagnostics.</param>
//==================================================================================================
public class UserService(IUserRepository repository, ILogger<UserService> logger)
{
    #region Public methods

    //==============================================================================================
    /// <summary>
    /// Creates a new user using functional pipeline.
    /// </summary>
    /// <param name="email">User email address.</param>
    /// <param name="password">User password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created user ID on success.</returns>
    /// <remarks>
    /// ValidateUserInput → ToResult → EnsureAsync → BindAsync → TapAsync → TapFailureAsync → MapAsync.
    /// </remarks>
    //==============================================================================================
    public async Task<Result<Guid>> CreateUserAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        return await ValidateUserInput(email, password)
            .ToResult((Email: email, Password: password))
            .EnsureAsync(_ => CheckEmailNotExistsAsync(email, cancellationToken), ResultError.Create("USER_EMAIL_EXISTS", "Email already exists"))
            .BindAsync(credentials => CreateAndSaveUserAsync(credentials.Email, credentials.Password, cancellationToken))
            .TapAsync(user => Task.Run(() => logger.LogInformation("User created: {UserId} - {Email}", user.Id, user.Email)))
            .TapFailureAsync(error => Task.Run(() => logger.LogError("User creation failed: {Error}", error)))
            .MapAsync(user => Task.FromResult(user.Id));
    }

    //==============================================================================================
    /// <summary>
    /// Retrieves user and transforms to DTO using functional pipeline.
    /// </summary>
    /// <param name="userId">User ID to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User DTO on success.</returns>
    /// <remarks>
    /// ToResult → Ensure → Map → Tap.
    /// </remarks>
    //==============================================================================================
    public async Task<Result<UserDto>> GetUserDtoAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return (await repository.FindByIdAsync(userId, cancellationToken))
            .ToResult(ResultError.Create("USER_NOT_FOUND", $"User {userId} not found"))
            .Ensure(u => u.IsActive, ResultError.Create("USER_INACTIVE", "User is not active"))
            .Map(u => new UserDto(u.Id, u.Email, u.IsActive, u.CreatedAt))
            .Tap(dto => logger.LogInformation("Retrieved user DTO: {UserId} - {Email}", dto.Id, dto.Email));
    }

    //==============================================================================================
    /// <summary>
    /// Updates user status using Bind for operation chaining.
    /// </summary>
    /// <param name="userId">User ID to update.</param>
    /// <param name="isActive">New status value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated user on success.</returns>
    /// <remarks>
    /// FindUserResult → BindAsync → TapAsync.
    /// </remarks>
    //==============================================================================================
    public async Task<Result<User>> UpdateUserStatusAsync(Guid userId, bool isActive, CancellationToken cancellationToken = default)
    {
        return await FindUserResult(userId, cancellationToken)
            .BindAsync(user => Task.FromResult(UpdateStatus(user, isActive)))
            .TapAsync(user => SaveUserAsync(user, cancellationToken));
    }

    //==============================================================================================
    /// <summary>
    /// Validates user input using fail-fast validation chain.
    /// </summary>
    /// <param name="email">Email to validate.</param>
    /// <param name="password">Password to validate.</param>
    /// <returns>Result.Success if valid, Result.Failure otherwise.</returns>
    /// <remarks>
    /// ValidateEmail → Bind → ValidatePassword (true fail-fast: stops at first error).
    /// Password validation only executes if email validation succeeds (lazy evaluation).
    /// </remarks>
    //==============================================================================================
    public Result ValidateUserInput(string email, string password)
    {
        return ValidateEmail(email)
            .Bind(() => ValidatePassword(password));
    }

    //==============================================================================================
    /// <summary>
    /// Validates user input collecting all validation errors.
    /// </summary>
    /// <param name="email">Email to validate.</param>
    /// <param name="password">Password to validate.</param>
    /// <returns>ErrorsResult with all validation errors collected.</returns>
    /// <remarks>
    /// ValidateEmail + ValidatePassword → ErrorsResult.Collect (collects all errors).
    /// Use this for better UX - shows all validation errors at once.
    /// </remarks>
    //==============================================================================================
    public ErrorsResult ValidateUserInputWithErrors(string email, string password)
    {
        return ErrorsResult.Collect(
            ValidateEmail(email),
            ValidatePassword(password));
    }

    //==============================================================================================
    /// <summary>
    /// Demonstrates complete functional pipeline using all extensions.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User summary on success, error message on failure.</returns>
    /// <remarks>
    /// FindUserResult → EnsureAsync → MapAsync → TapAsync → MatchAsync.
    /// </remarks>
    //==============================================================================================
    public async Task<string> GetUserSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await FindUserResult(userId, cancellationToken)
            .EnsureAsync(user => Task.FromResult(user.IsActive), "User is inactive")
            .MapAsync(user => Task.FromResult($"{user.Email} (Active since {user.CreatedAt:yyyy-MM-dd})"))
            .TapAsync(summary => Task.Run(() => logger.LogInformation("Generated summary: {Summary}", summary)))
            .MatchAsync(
                onSuccess: summary => Task.FromResult(summary),
                onFailure: error => Task.FromResult($"Error: {error}")
            );
    }

    #endregion

    #region Private methods

    //==============================================================================================
    /// <summary>
    /// Checks if email does not exist in the system.
    /// </summary>
    /// <param name="email">Email to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if email does not exist, false otherwise.</returns>
    //==============================================================================================
    private async Task<bool> CheckEmailNotExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await repository.FindByEmailAsync(email, cancellationToken) == null;
    }

    //==============================================================================================
    /// <summary>
    /// Creates new user and saves to repository.
    /// </summary>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New user object on success.</returns>
    //==============================================================================================
    private async Task<Result<User>> CreateAndSaveUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = new User(email, password);
        await repository.AddAsync(user, cancellationToken);
        return Result<User>.Success(user);
    }

    //==============================================================================================
    /// <summary>
    /// Validates email address.
    /// </summary>
    /// <param name="email">Email to validate.</param>
    /// <returns>Result.Success if valid, Result.Failure otherwise.</returns>
    /// <remarks>
    /// Result.Success → Ensure(not empty) → Ensure(contains @).
    /// </remarks>
    //==============================================================================================
    private static Result ValidateEmail(string email)
    {
        return Result.Success()
            .Ensure(() => !string.IsNullOrWhiteSpace(email), ResultError.Create("VALIDATION_EMAIL_REQUIRED", "Email is required"))
            .Ensure(() => email.Contains('@'), ResultError.Create("VALIDATION_EMAIL_FORMAT", "Invalid email format"));
    }

    //==============================================================================================
    /// <summary>
    /// Validates password.
    /// </summary>
    /// <param name="password">Password to validate.</param>
    /// <returns>Result.Success if valid, Result.Failure otherwise.</returns>
    /// <remarks>
    /// Result.Success → Ensure(not empty) → Ensure(min 8 chars).
    /// </remarks>
    //==============================================================================================
    private static Result ValidatePassword(string password)
    {
        return Result.Success()
            .Ensure(() => !string.IsNullOrWhiteSpace(password), ResultError.Create("VALIDATION_PASSWORD_REQUIRED", "Password is required"))
            .Ensure(() => password.Length >= 8, ResultError.Create("VALIDATION_PASSWORD_LENGTH", "Password must be at least 8 characters"));
    }

    //==============================================================================================
    /// <summary>
    /// Finds user by ID and wraps in Result.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User on success, Failure if not found.</returns>
    //==============================================================================================
    private async Task<Result<User>> FindUserResult(Guid userId, CancellationToken cancellationToken)
    {
        var user = await repository.FindByIdAsync(userId, cancellationToken);
        return user != null
            ? Result<User>.Success(user)
            : Result<User>.Failure($"User {userId} not found");
    }

    //==============================================================================================
    /// <summary>
    /// Updates user active status using domain encapsulation.
    /// </summary>
    /// <param name="user">User object.</param>
    /// <param name="isActive">New status value.</param>
    /// <returns>Updated user wrapped in Result.</returns>
    /// <remarks>
    /// Domain encapsulation: Uses Activate/Deactivate methods instead of property setters.
    /// </remarks>
    //==============================================================================================
    private static Result<User> UpdateStatus(User user, bool isActive)
    {
        if (isActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        return Result<User>.Success(user);
    }

    //==============================================================================================
    /// <summary>
    /// Saves user changes to repository.
    /// </summary>
    /// <param name="user">User to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completed task.</returns>
    //==============================================================================================
    private async Task SaveUserAsync(User user, CancellationToken cancellationToken)
    {
        await repository.UpdateAsync(user, cancellationToken);
        logger.LogInformation("User {UserId} status updated to {IsActive}", user.Id, user.IsActive);
    }

    #endregion
}
