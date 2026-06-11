using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeLeaveSystem.Api.Models.Entities;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public int? ManagerId { get; set; }

    [MaxLength(100)]
    public string? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public Role Role { get; set; } = null!;

    [ForeignKey("ManagerId")]
    public User? Manager { get; set; }

    public ICollection<User> Subordinates { get; set; } = new List<User>();

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    public ICollection<LeaveRequest> ReviewedRequests { get; set; } = new List<LeaveRequest>();

    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
}
