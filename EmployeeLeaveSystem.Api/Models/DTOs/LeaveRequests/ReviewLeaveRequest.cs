using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveSystem.Api.Models.DTOs.LeaveRequests;

public class ReviewLeaveRequest
{
    [Required]
    public bool IsApproved { get; set; }

    [MaxLength(500)]
    public string? ReviewComment { get; set; }
}
