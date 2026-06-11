using EmployeeLeaveSystem.Api.Models.DTOs.Auth;

namespace EmployeeLeaveSystem.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> Login(LoginRequest request);
    Task<bool> Register(RegisterRequest request);
}
