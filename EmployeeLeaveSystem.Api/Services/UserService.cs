using Microsoft.EntityFrameworkCore;
using EmployeeLeaveSystem.Api.Data;
using EmployeeLeaveSystem.Api.Models.DTOs.Users;
using EmployeeLeaveSystem.Api.Models.Entities;

namespace EmployeeLeaveSystem.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UserService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<UserResponse>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .ToListAsync();
        return users.Select(u => MapToResponse(u)).ToList();
    }

    public async Task<UserResponse?> GetUserByIdAsync(int id)
    {
        if (_currentUser.Role != "Admin" && _currentUser.UserId != id)
            return null;

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user is null ? null : MapToResponse(user);
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null)
            return false;

        if (request.FirstName is not null)
            user.FirstName = request.FirstName;

        if (request.LastName is not null)
            user.LastName = request.LastName;

        if (request.Department is not null)
            user.Department = request.Department;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        if (request.ManagerId.HasValue)
            user.ManagerId = request.ManagerId;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user is null)
            return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<UserResponse>> GetTeamMembersAsync()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.ManagerId == _currentUser.UserId)
            .ToListAsync();
        return users.Select(u => MapToResponse(u)).ToList();
    }

    private static UserResponse MapToResponse(User user) => new()
    {
        Id = user.Id,
        FullName = $"{user.FirstName} {user.LastName}",
        Email = user.Email,
        RoleName = user.Role.Name,
        Department = user.Department,
        IsActive = user.IsActive,
        ManagerId = user.ManagerId,
        CreatedAt = user.CreatedAt
    };
}
