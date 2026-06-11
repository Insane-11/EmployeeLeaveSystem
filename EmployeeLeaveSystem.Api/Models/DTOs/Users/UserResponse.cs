namespace EmployeeLeaveSystem.Api.Models.DTOs.Users;

public class UserResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public bool IsActive { get; set; }
    public int? ManagerId { get; set; }
    public DateTime CreatedAt { get; set; }
}
