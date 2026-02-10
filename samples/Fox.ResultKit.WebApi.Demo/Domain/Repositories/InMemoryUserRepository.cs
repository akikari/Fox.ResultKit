//==================================================================================================
// In-memory user repository implementation for demonstration purposes.
// Thread-safe user storage using ConcurrentDictionary.
//==================================================================================================
using System.Collections.Concurrent;
using Fox.ResultKit.WebApi.Demo.Domain.Models;

namespace Fox.ResultKit.WebApi.Demo.Domain.Repositories;

//==================================================================================================
/// <summary>
/// In-memory user repository implementation (demo purposes).
/// </summary>
//==================================================================================================
public class InMemoryUserRepository : IUserRepository
{
    //==============================================================================================
    /// <summary>
    /// Thread-safe user storage (in-memory).
    /// </summary>
    //==============================================================================================
    private readonly ConcurrentDictionary<Guid, User> users = new();

    //==============================================================================================
    /// <summary>
    /// Adds new user to repository.
    /// </summary>
    /// <param name="user">User to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completed task.</returns>
    //==============================================================================================
    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        users.TryAdd(user.Id, user);
        return Task.CompletedTask;
    }

    //==============================================================================================
    /// <summary>
    /// Finds user by ID.
    /// </summary>
    /// <param name="id">User ID to find.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User if found, null otherwise.</returns>
    //==============================================================================================
    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    //==============================================================================================
    /// <summary>
    /// Finds user by email.
    /// </summary>
    /// <param name="email">Email to find.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>User if found, null otherwise.</returns>
    //==============================================================================================
    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        var user = users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    //==============================================================================================
    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of all users.</returns>
    //==============================================================================================
    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<User> userList = [.. users.Values];
        return Task.FromResult(userList);
    }
}
