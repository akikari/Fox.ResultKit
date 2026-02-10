//==================================================================================================
// User Data Transfer Objects for API responses.
// Record types for immutable user data representation.
//==================================================================================================

namespace Fox.ResultKit.WebApi.Demo.DTOs;

//==================================================================================================
/// <summary>
/// User DTO.
/// </summary>
//==================================================================================================
public record UserDto(Guid Id, string Email, bool IsActive, DateTime CreatedAt);

//==================================================================================================
/// <summary>
/// User creation request.
/// </summary>
//==================================================================================================
public record CreateUserRequest(string Email, string Password);

//==================================================================================================
/// <summary>
/// User status update request.
/// </summary>
//==================================================================================================
public record UpdateUserStatusRequest(bool IsActive);
