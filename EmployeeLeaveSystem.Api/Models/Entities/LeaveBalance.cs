using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeLeaveSystem.Api.Models.Entities;

public class LeaveBalance
{
    public int Id { get; set; }

    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string LeaveType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(4,1)")]
    public decimal TotalDays { get; set; }

    [Column(TypeName = "decimal(4,1)")]
    public decimal UsedDays { get; set; }

    [Column(TypeName = "decimal(4,1)")]
    public decimal RemainingDays { get; set; }

    public int Year { get; set; }

    public User User { get; set; } = null!;
}
