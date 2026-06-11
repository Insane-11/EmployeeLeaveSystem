using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveSystem.Api.Models.DTOs.Users;

public class UpdateUserRequest
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    public bool? IsActive { get; set; }

    public int? ManagerId { get; set; }
}
