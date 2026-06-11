namespace EmployeeLeaveSystem.Api.Services;

public interface ICurrentUserService
{
    int UserId { get; }
    string? Role { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
