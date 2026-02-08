//==================================================================================================
// ListUsers query and handler demonstrating CQRS with Map transformation.
// Query pattern implementation with collection mapping.
//==================================================================================================
using Fox.ResultKit.WebApi.Demo.Domain.Repositories;
using Fox.ResultKit.WebApi.Demo.DTOs;
using MediatR;

namespace Fox.ResultKit.WebApi.Demo.Features.Users.ListUsers;

//==================================================================================================
/// <summary>
/// ListUsers query.
/// </summary>
//==================================================================================================
public record ListUsersQuery : IRequest<Result<IReadOnlyList<UserDto>>>;

//==================================================================================================
/// <summary>
/// ListUsers query handler demonstrating Map collection transformation.
/// </summary>
/// <param name="repository">Repository for user data access.</param>
/// <param name="logger">Logger instance for diagnostics.</param>
//==================================================================================================
public class ListUsersQueryHandler(IUserRepository repository, ILogger<ListUsersQueryHandler> logger) : IRequestHandler<ListUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    //==============================================================================================
    /// <summary>
    /// Handles ListUsers query.
    /// </summary>
    /// <param name="request">ListUsers query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of user DTOs.</returns>
    /// <remarks>
    /// Repository.GetAllAsync → Result.Success → Map (User[] → UserDto[]) → Tap (logging).
    /// </remarks>
    //==============================================================================================
    public async Task<Result<IReadOnlyList<UserDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await repository.GetAllAsync(cancellationToken);

        return Result<IReadOnlyList<Domain.Models.User>>.Success(users)
            .Map(list => (IReadOnlyList<UserDto>)[.. list.Select(u => new UserDto(u.Id, u.Email, u.IsActive, u.CreatedAt))])
            .Tap(dtos => logger.LogInformation("Retrieved {Count} users via CQRS", dtos.Count));
    }
}
