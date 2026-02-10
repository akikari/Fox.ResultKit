//==================================================================================================
// GetUser query and handler demonstrating CQRS with functional pipeline.
// Query pattern implementation using Railway Oriented Programming.
//==================================================================================================
using Fox.ResultKit.WebApi.Demo.Domain.Repositories;
using Fox.ResultKit.WebApi.Demo.DTOs;
using MediatR;

namespace Fox.ResultKit.WebApi.Demo.Features.Users.GetUser;

//==================================================================================================
/// <summary>
/// GetUser query.
/// </summary>
/// <param name="UserId">User ID to retrieve.</param>
//==================================================================================================
public record GetUserQuery(Guid UserId) : IRequest<Result<UserDto>>;

//==================================================================================================
/// <summary>
/// GetUser query handler demonstrating functional pipeline.
/// </summary>
/// <param name="repository">Repository for user data access.</param>
/// <param name="logger">Logger instance for diagnostics.</param>
//==================================================================================================
public class GetUserQueryHandler(IUserRepository repository, ILogger<GetUserQueryHandler> logger) : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    //==============================================================================================
    /// <summary>
    /// Complete functional pipeline example in MediatR handler.
    /// </summary>
    /// <param name="request">GetUser query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User DTO on success; Result.Failure on error.</returns>
    /// <remarks>
    /// Demonstrates: FindByIdAsync → Ensure → Map → Tap pattern.
    /// </remarks>
    //==============================================================================================
    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await repository.FindByIdAsync(request.UserId, cancellationToken);

        return Result<Domain.Models.User>.Success(user!)
            .Ensure(u => u != null, $"User {request.UserId} not found")
            .Ensure(u => u!.IsActive, "User is not active")
            .Map(u => new UserDto(u!.Id, u.Email, u.IsActive, u.CreatedAt))
            .Tap(dto => logger.LogInformation("Retrieved user via CQRS: {UserId} - {Email}", dto.Id, dto.Email));
    }
}
