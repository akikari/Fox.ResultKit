//==================================================================================================
// CreateUser command and handler demonstrating CQRS with MediatR.
// Command pattern implementation with ResultKit integration.
//==================================================================================================
using Fox.ResultKit.WebApi.Demo.Domain.Models;
using Fox.ResultKit.WebApi.Demo.Domain.Repositories;
using MediatR;

namespace Fox.ResultKit.WebApi.Demo.Features.Users.CreateUser;

//==================================================================================================
/// <summary>
/// CreateUser command.
/// </summary>
/// <param name="Email">User email address.</param>
/// <param name="Password">User password.</param>
//==================================================================================================
public record CreateUserCommand(string Email, string Password) : IRequest<Result<Guid>>;

//==================================================================================================
/// <summary>
/// CreateUser command handler.
/// </summary>
/// <param name="repository">Repository for user data access.</param>
/// <param name="logger">Logger instance for diagnostics.</param>
//==================================================================================================
public class CreateUserCommandHandler(IUserRepository repository, ILogger<CreateUserCommandHandler> logger) : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    //==============================================================================================
    /// <summary>
    /// Handles CreateUser command.
    /// </summary>
    /// <param name="request">CreateUser command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created user ID on success; Result.Failure on validation error.</returns>
    /// <remarks>
    /// ValidateCommand → EnsureAsync (email uniqueness) → BindAsync (User creation + AddAsync) → TapAsync (logging).
    /// </remarks>
    //==============================================================================================
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return await ValidateCommand(request)
            .ToResult(request)
            .EnsureAsync(_ => CheckEmailNotExistsAsync(request.Email, cancellationToken), "Email already exists")
            .BindAsync(_ => CreateAndSaveUserAsync(request, cancellationToken))
            .TapAsync(userId => Task.Run(() => logger.LogInformation("User created via CQRS: {UserId} - {Email}", userId, request.Email)));
    }

    //==============================================================================================
    /// <summary>
    /// Validates CreateUser command (combined email and password validation).
    /// </summary>
    /// <param name="command">Command to validate.</param>
    /// <returns>Result.Success if valid, Result.Failure otherwise.</returns>
    /// <remarks>
    /// ValidateEmail + ValidatePassword → Combine.
    /// </remarks>
    //==============================================================================================
    private static Result ValidateCommand(CreateUserCommand command)
    {
        return ResultCombineExtensions.Combine(
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
    /// <remarks>
    /// Result.Success → Ensure(not empty) → Ensure(contains @).
    /// </remarks>
    //==============================================================================================
    private static Result ValidateEmail(string email)
    {
        return Result.Success()
            .Ensure(() => !string.IsNullOrWhiteSpace(email), "Email is required")
            .Ensure(() => email.Contains('@'), "Invalid email format");
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
            .Ensure(() => !string.IsNullOrWhiteSpace(password), "Password is required")
            .Ensure(() => password.Length >= 8, "Password must be at least 8 characters");
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
    /// <param name="request">CreateUser command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New user ID on success.</returns>
    //==============================================================================================
    private async Task<Result<Guid>> CreateAndSaveUserAsync(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User(request.Email, request.Password);
        await repository.AddAsync(user, cancellationToken);
        return Result<Guid>.Success(user.Id);
    }
}
