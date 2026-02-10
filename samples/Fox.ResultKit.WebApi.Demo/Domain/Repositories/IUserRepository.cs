//==================================================================================================
// User repository interface for data access abstraction.
// Contract for user persistence operations.
//==================================================================================================
using Fox.ResultKit.WebApi.Demo.Domain.Models;

namespace Fox.ResultKit.WebApi.Demo.Domain.Repositories;

//==================================================================================================
/// <summary>
/// User repository interface.
/// </summary>
//==================================================================================================
public interface IUserRepository
{
    //==============================================================================================
    /// <summary>
    /// Adds a user.
    /// </summary>
    /// <param name="user">User to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    //==============================================================================================
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    //==============================================================================================
    /// <summary>
    /// Finds user by ID.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User if exists, null otherwise.</returns>
    //==============================================================================================
    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    //==============================================================================================
    /// <summary>
    /// Finds user by email.
    /// </summary>
    /// <param name="email">Email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User if exists, null otherwise.</returns>
    //==============================================================================================
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    //==============================================================================================
    /// <summary>
    /// Updates user in the repository.
    /// </summary>
    /// <param name="user">User to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    //==============================================================================================
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    //==============================================================================================
    /// <summary>
    /// Returns all users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of all users.</returns>
    //==============================================================================================
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
}
