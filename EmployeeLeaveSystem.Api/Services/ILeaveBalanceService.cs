using EmployeeLeaveSystem.Api.Models.Entities;

namespace EmployeeLeaveSystem.Api.Services;

public interface ILeaveBalanceService
{
    Task<LeaveBalance> GetOrCreateBalanceAsync(int userId, string leaveType, int year);
    Task<bool> DeductBalanceAsync(int userId, string leaveType, decimal days, int year);
    Task<decimal> GetRemainingDaysAsync(int userId, string leaveType, int year);
    Task<List<LeaveBalance>> GetUserBalancesAsync(int userId, int? year);
}
