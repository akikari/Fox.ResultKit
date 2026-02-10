//==================================================================================================
// Classic service-based controller demonstrating ResultKit with dependency injection.
// ASP.NET Core Web API integration with Railway Oriented Programming patterns.
//==================================================================================================
using Fox.ResultKit.WebApi.Demo.Domain.Models;
using Fox.ResultKit.WebApi.Demo.Domain.Repositories;
using Fox.ResultKit.WebApi.Demo.Domain.Services;
using Fox.ResultKit.WebApi.Demo.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Fox.ResultKit.WebApi.Demo.Controllers;

//==================================================================================================
/// <summary>
/// Classic service-based controller demonstrating ResultKit usage.
/// </summary>
/// <param name="userService">User service handling user operations.</param>
/// <param name="repository">User repository for direct data access in validation demo.</param>
//==================================================================================================
[ApiController]
[Route("api/classic/users")]
[Tags("Classic Service Layer")]
public class ClassicUsersController(UserService userService, IUserRepository repository) : ControllerBase
{
    //==============================================================================================
    /// <summary>
    /// Creates a new user demonstrating basic Result usage.
    /// </summary>
    /// <param name="request">User creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK with user ID on success; 400 BadRequest on validation error.</returns>
    /// <remarks>
    /// UserService.CreateUserAsync → Match-based HTTP response mapping (Ok/BadRequest).
    /// </remarks>
    //==============================================================================================
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await userService.CreateUserAsync(request.Email, request.Password, cancellationToken);

        return result.Match<Guid, IActionResult>(
            onSuccess: userId => Ok(new { userId }),
            onFailure: error =>
            {
                var (code, message) = ResultError.Parse(error);
                return code switch
                {
                    "USER_EMAIL_EXISTS" => Conflict(new { error = message, code }),
                    _ => BadRequest(new { error = message, code = string.IsNullOrEmpty(code) ? null : code })
                };
            }
        );
    }

    //==============================================================================================
    /// <summary>
    /// Creates a new user demonstrating ErrorsResult for collecting all validation errors.
    /// </summary>
    /// <param name="request">User creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 Created with user ID on success; 400 BadRequest with all validation errors.</returns>
    /// <remarks>
    /// Demonstrates separation of concerns:
    /// 1. Validation phase: ErrorsResult.Collect (show ALL errors at once)
    /// 2. Domain pipeline: Result fail-fast (business logic errors)
    /// Better UX - user sees all validation problems immediately.
    /// </remarks>
    //==============================================================================================
    [HttpPost("with-validation")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUserWithValidation([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Phase 1: Input validation - collect ALL errors
        var validation = userService.ValidateUserInputWithErrors(request.Email, request.Password);

        if (validation.IsFailure)
        {
            var errors = validation.Errors
                .Select(ResultError.Parse)
                .Select(e => new { code = e.Code, message = e.Message })
                .ToList();

            return BadRequest(new { errors });
        }

        // Phase 2: Domain operations - fail-fast pipeline
        var emailExistsCheck = await CheckEmailNotExistsAsync(request.Email, cancellationToken);
        if (!emailExistsCheck)
        {
            return Conflict(new
            {
                error = "Email already exists",
                code = "USER_EMAIL_EXISTS"
            });
        }

        var userId = await CreateAndSaveUserAsync(request.Email, request.Password, cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = userId }, new { userId });
    }

    //==============================================================================================
    /// <summary>
    /// Retrieves user demonstrating Map, Ensure, Tap pipeline.
    /// </summary>
    /// <param name="id">User ID to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK with UserDto on success; 404 NotFound if not found.</returns>
    /// <remarks>
    /// UserService.GetUserDtoAsync → Match (Ok/NotFound mapping).
    /// </remarks>
    //==============================================================================================
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var result = await userService.GetUserDtoAsync(id, cancellationToken);

        return result.Match<UserDto, IActionResult>(
            onSuccess: user => Ok(user),
            onFailure: error =>
            {
                var (code, message) = ResultError.Parse(error);
                return code switch
                {
                    "USER_NOT_FOUND" => NotFound(new { error = message, code }),
                    "USER_INACTIVE" => StatusCode(StatusCodes.Status403Forbidden, new { error = message, code }),
                    _ => BadRequest(new { error = message, code = string.IsNullOrEmpty(code) ? null : code })
                };
            }
        );
    }

    //==============================================================================================
    /// <summary>
    /// Updates user status demonstrating Bind pipeline.
    /// </summary>
    /// <param name="id">User ID to update.</param>
    /// <param name="request">New status value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 NoContent on success; 404 NotFound if not found.</returns>
    /// <remarks>
    /// UserService.UpdateUserStatusAsync → Match-based HTTP response mapping (NoContent/NotFound).
    /// </remarks>
    //==============================================================================================
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return (await userService.UpdateUserStatusAsync(id, request.IsActive, cancellationToken))
            .Match<User, IActionResult>(
                onSuccess: _ => NoContent(),
                onFailure: error => NotFound(new { error })
            );
    }

    //==============================================================================================
    /// <summary>
    /// Retrieves user summary demonstrating complete functional pipeline with Match.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK with user summary (success or error message).</returns>
    /// <remarks>
    /// UserService.GetUserSummaryAsync → Ok response wrap.
    /// </remarks>
    //==============================================================================================
    [HttpGet("{id:guid}/summary")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserSummary(Guid id, CancellationToken cancellationToken)
    {
        var summary = await userService.GetUserSummaryAsync(id, cancellationToken);
        return Ok(new { summary });
    }

    #region Private helper methods for CreateUserWithValidation

    private async Task<bool> CheckEmailNotExistsAsync(string email, CancellationToken cancellationToken)
    {
        var existingUser = await repository.FindByEmailAsync(email, cancellationToken);
        return existingUser == null;
    }

    private async Task<Guid> CreateAndSaveUserAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = new User(email, password);
        await repository.AddAsync(user, cancellationToken);
        return user.Id;
    }

    #endregion
}
