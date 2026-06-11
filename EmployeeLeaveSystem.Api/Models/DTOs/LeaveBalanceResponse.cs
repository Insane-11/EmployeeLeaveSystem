namespace EmployeeLeaveSystem.Api.Models.DTOs;

public class LeaveBalanceResponse
{
    public string LeaveType { get; set; } = string.Empty;
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public int Year { get; set; }
}
