using EmployeeLeaveSystem.Api.Models;
using EmployeeLeaveSystem.Api.Models.DTOs.LeaveRequests;

namespace EmployeeLeaveSystem.Api.Services;

public interface ILeaveRequestService
{
    Task<ServiceResult<LeaveRequestResponse>> CreateLeaveRequestAsync(CreateLeaveRequest request, int employeeId);
    Task<List<LeaveRequestResponse>> GetMyLeaveRequestsAsync(int employeeId);
    Task<LeaveRequestResponse?> GetLeaveRequestByIdAsync(int id);
    Task<ServiceResult<bool>> UpdateLeaveRequestAsync(int id, UpdateLeaveRequest request, int employeeId);
    Task<bool> CancelLeaveRequestAsync(int id, int employeeId);
    Task<List<LeaveRequestResponse>> GetTeamLeaveRequestsAsync(int managerId);
    Task<List<LeaveRequestResponse>> GetAllLeaveRequestsAsync();
    Task<List<LeaveRequestResponse>> GetPendingLeaveRequestsAsync();
    Task<bool> ApproveLeaveRequestAsync(int id, int reviewerId, string? comment);
    Task<bool> RejectLeaveRequestAsync(int id, int reviewerId, string? comment);
}
