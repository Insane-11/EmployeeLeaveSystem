namespace EmployeeLeaveSystem.Api.Models.DTOs.LeaveRequests;

public class LeaveRequestResponse
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal DurationDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
