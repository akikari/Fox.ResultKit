//==================================================================================================
// CQRS controller demonstrating MediatR integration with ResultKit.
// Command Query Responsibility Segregation pattern implementation.
//==================================================================================================
using Fox.ResultKit.WebApi.Demo.DTOs;
using Fox.ResultKit.WebApi.Demo.Features.Users.CreateUser;
using Fox.ResultKit.WebApi.Demo.Features.Users.CreateUserWithValidation;
using Fox.ResultKit.WebApi.Demo.Features.Users.GetUser;
using Fox.ResultKit.WebApi.Demo.Features.Users.ListUsers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fox.ResultKit.WebApi.Demo.Controllers;

//==================================================================================================
/// <summary>
/// CQRS controller with MediatR and ResultKit integration.
/// </summary>
/// <param name="mediator">MediatR instance for sending commands/queries.</param>
//==================================================================================================
[ApiController]
[Route("api/cqrs/users")]
[Tags("CQRS with MediatR")]
public class CqrsUsersController(IMediator mediator) : ControllerBase
{
    //==============================================================================================
    /// <summary>
    /// Creates new user via CQRS command.
    /// </summary>
    /// <param name="request">User creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 Created with user ID on success; 400 BadRequest on validation error.</returns>
    /// <remarks>
    /// CreateUserCommand → MediatR.Send → Match-based HTTP response mapping (CreatedAtAction/BadRequest).
    /// </remarks>
    //==============================================================================================
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new CreateUserCommand(request.Email, request.Password);
        var result = await mediator.Send(command, cancellationToken);

        return result.Match<Guid, IActionResult>(
            onSuccess: userId => CreatedAtAction(nameof(GetUser), new { id = userId }, new { userId }),
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
    /// Creates new user via CQRS command demonstrating ErrorsResult validation pattern.
    /// </summary>
    /// <param name="request">User creation data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 Created with user ID on success; 400 BadRequest with all validation errors.</returns>
    /// <remarks>
    /// CreateUserWithValidationCommand → MediatR.Send → Demonstrates separation:
    /// 1. Validation phase: ErrorsResult collects ALL errors
    /// 2. Domain phase: Result fail-fast for business logic
    /// Better UX - user sees all validation problems at once.
    /// </remarks>
    //==============================================================================================
    [HttpPost("with-validation")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUserWithValidation([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new CreateUserWithValidationCommand(request.Email, request.Password);
        var result = await mediator.Send(command, cancellationToken);

        return result.Match<Guid, IActionResult>(
            onSuccess: userId => CreatedAtAction(nameof(GetUser), new { id = userId }, new { userId }),
            onFailure: error =>
            {
                // Check if error contains multiple validation errors (newline separated)
                if (error.Contains(Environment.NewLine))
                {
                    // Multiple validation errors - parse and return array
                    var errors = error.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                        .Select(ResultError.Parse)
                        .Select(e => new { code = e.Code, message = e.Message })
                        .ToList();

                    return BadRequest(new { errors });
                }

                // Single error - standard handling
                var (code, message) = ResultError.Parse(error);
                return code switch
                {
                    "USER_EMAIL_EXISTS" => Conflict(new { error = message, code }),
                    _ => BadRequest(new { error = message, code })
                };
            }
        );
    }

    //==============================================================================================
    /// <summary>
    /// Retrieves user via CQRS query with functional pipeline.
    /// </summary>
    /// <param name="id">User ID to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK with UserDto on success; 404 NotFound if not found.</returns>
    /// <remarks>
    /// GetUserQuery → MediatR.Send → Match-based HTTP response mapping (Ok/NotFound).
    /// </remarks>
    //==============================================================================================
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        return result.Match<UserDto, IActionResult>(
            onSuccess: user => Ok(user),
            onFailure: error => NotFound(new { error })
        );
    }

    //==============================================================================================
    /// <summary>
    /// Lists all users via CQRS query with Map demonstration.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>200 OK with user list; 500 Internal Server Error on failure.</returns>
    /// <remarks>
    /// ListUsersQuery → MediatR.Send → Match-based HTTP response mapping (Ok/StatusCode 500).
    /// </remarks>
    //==============================================================================================
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListUsers(CancellationToken cancellationToken)
    {
        var query = new ListUsersQuery();
        var result = await mediator.Send(query, cancellationToken);

        return result.Match<IReadOnlyList<UserDto>, IActionResult>(
            onSuccess: users => Ok(users),
            onFailure: error => StatusCode(500, new { error })
        );
    }
}
