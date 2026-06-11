using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveSystem.Api.Models.Entities;

public class LeaveRequestStatus
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
