using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeLeaveSystem.Api.Models.Entities;

public class LeaveRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    [Required]
    [MaxLength(50)]
    public string LeaveType { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Column(TypeName = "decimal(4,1)")]
    public decimal DurationDays { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    public int StatusId { get; set; }

    public int? ReviewedById { get; set; }

    [MaxLength(500)]
    public string? ReviewComment { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public User Employee { get; set; } = null!;

    [ForeignKey("ReviewedById")]
    public User? Reviewer { get; set; }

    public LeaveRequestStatus Status { get; set; } = null!;
}
