using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveSystem.Api.Models.DTOs.LeaveRequests;

public class CreateLeaveRequest
{
    [Required]
    public string LeaveType { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }
}
