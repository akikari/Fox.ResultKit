//==================================================================================================
// CreateUserWithValidation command demonstrating ErrorsResult pattern in CQRS.
// Shows separation of validation phase and domain operations.
//==================================================================================================
using Fox.ResultKit.WebApi.Demo.Domain.Models;
using Fox.ResultKit.WebApi.Demo.Domain.Repositories;
using MediatR;

namespace Fox.ResultKit.WebApi.Demo.Features.Users.CreateUserWithValidation;

//==================================================================================================
/// <summary>
/// CreateUserWithValidation command demonstrating ErrorsResult for validation.
/// </summary>
/// <param name="Email">User email address.</param>
/// <param name="Password">User password.</param>
//==================================================================================================
public record CreateUserWithValidationCommand(string Email, string Password) : IRequest<Result<Guid>>;

//==================================================================================================
/// <summary>
/// CreateUserWithValidation command handler showing validation-first pattern.
/// </summary>
/// <param name="repository">Repository for user data access.</param>
/// <param name="logger">Logger instance for diagnostics.</param>
/// <remarks>
/// Demonstrates best practice:
/// Phase 1: Input validation with ErrorsResult.Collect (show all errors)
/// Phase 2: Domain operations with Result fail-fast (business logic)
/// </remarks>
//==================================================================================================
public class CreateUserWithValidationCommandHandler(IUserRepository repository, ILogger<CreateUserWithValidationCommandHandler> logger)
    : IRequestHandler<CreateUserWithValidationCommand, Result<Guid>>
{
    #region Public methods

    //==============================================================================================
    /// <summary>
    /// Handles CreateUserWithValidation command with separate validation and domain phases.
    /// </summary>
    /// <param name="request">CreateUserWithValidation command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created user ID on success; Result.Failure on error.</returns>
    //==============================================================================================
    public async Task<Result<Guid>> Handle(CreateUserWithValidationCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Phase 1: Input validation - collect ALL errors (better UX)
        var validation = ValidateCommandWithErrors(request);
        if (validation.IsFailure)
        {
            // Convert ErrorsResult to Result for return type compatibility
            return Result<Guid>.Failure(string.Join(Environment.NewLine, validation.Errors));
        }

        // Phase 2: Domain operations - fail-fast pipeline
        var emailExistsCheck = await CheckEmailNotExistsAsync(request.Email, cancellationToken);
        if (!emailExistsCheck)
        {
            return Result<Guid>.Failure(ResultError.Create("USER_EMAIL_EXISTS", "Email already exists"));
        }

        var createResult = await CreateAndSaveUserAsync(request, cancellationToken);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        logger.LogInformation("User created via CQRS with validation: {UserId} - {Email}", createResult.Value, request.Email);
        return createResult;
    }

    #endregion

    #region Private methods

    //==============================================================================================
    /// <summary>
    /// Validates CreateUserWithValidation command collecting all validation errors.
    /// </summary>
    /// <param name="command">Command to validate.</param>
    /// <returns>ErrorsResult with all validation errors collected.</returns>
    //==============================================================================================
    private static ErrorsResult ValidateCommandWithErrors(CreateUserWithValidationCommand command)
    {
        return ErrorsResult.Collect(
            ValidateEmail(command.Email),
            ValidatePassword(command.Password)
        );
    }

    //==============================================================================================
    /// <summary>
    /// Validates email address.
    /// </summary>
    /// <param name="email">Email to validate.</param>
    /// <returns>Result.Success if valid, Result.Failure otherwise.</returns>
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
    //==============================================================================================
    private static Result ValidatePassword(string password)
    {
        return Result.Success()
            .Ensure(() => !string.IsNullOrWhiteSpace(password), ResultError.Create("VALIDATION_PASSWORD_REQUIRED", "Password is required"))
            .Ensure(() => password.Length >= 8, ResultError.Create("VALIDATION_PASSWORD_LENGTH", "Password must be at least 8 characters"));
    }

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
    /// <param name="command">Command with user data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New user ID on success.</returns>
    //==============================================================================================
    private async Task<Result<Guid>> CreateAndSaveUserAsync(CreateUserWithValidationCommand command, CancellationToken cancellationToken)
    {
        var user = new User(command.Email, command.Password);
        await repository.AddAsync(user, cancellationToken);
        return Result<Guid>.Success(user.Id);
    }

    #endregion
}
