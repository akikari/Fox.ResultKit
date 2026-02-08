//==================================================================================================
// Classic service-based controller demonstrating ResultKit with dependency injection.
// ASP.NET Core Web API integration with Railway Oriented Programming patterns.
//==================================================================================================
using Fox.ResultKit.WebApi.Demo.Domain.Models;
using Fox.ResultKit.WebApi.Demo.Domain.Services;
using Fox.ResultKit.WebApi.Demo.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Fox.ResultKit.WebApi.Demo.Controllers;

//==================================================================================================
/// <summary>
/// Classic service-based controller demonstrating ResultKit usage.
/// </summary>
/// <param name="userService">User service handling user operations.</param>
//==================================================================================================
[ApiController]
[Route("api/classic/users")]
[Tags("Classic Service Layer")]
public class ClassicUsersController(UserService userService) : ControllerBase
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
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return (await userService.CreateUserAsync(request.Email, request.Password, cancellationToken))
            .Match<Guid, IActionResult>(
                onSuccess: userId => Ok(new { userId }),
                onFailure: error => BadRequest(new { error })
            );
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
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        return (await userService.GetUserDtoAsync(id, cancellationToken))
            .Match<UserDto, IActionResult>(
                onSuccess: user => Ok(user),
                onFailure: error => NotFound(new { error })
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
}
