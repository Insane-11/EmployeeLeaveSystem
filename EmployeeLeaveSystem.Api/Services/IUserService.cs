using EmployeeLeaveSystem.Api.Models.DTOs.Users;

namespace EmployeeLeaveSystem.Api.Services;

public interface IUserService
{
    Task<List<UserResponse>> GetAllUsersAsync();
    Task<UserResponse?> GetUserByIdAsync(int id);
    Task<bool> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(int id);
    Task<List<UserResponse>> GetTeamMembersAsync();
}
