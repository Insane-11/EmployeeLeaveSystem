using Microsoft.EntityFrameworkCore;
using EmployeeLeaveSystem.Api.Data;
using EmployeeLeaveSystem.Api.Models.DTOs.LeaveRequests;
using EmployeeLeaveSystem.Api.Models.Entities;
using EmployeeLeaveSystem.Api.Models.Enums;

namespace EmployeeLeaveSystem.Api.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public LeaveRequestService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<LeaveRequestResponse?> CreateLeaveRequestAsync(CreateLeaveRequest request, int employeeId)
    {
        var duration = (int)(request.EndDate - request.StartDate).TotalDays + 1;

        if (duration <= 0)
            return null;

        var hasOverlap = await _context.LeaveRequests
            .AnyAsync(l => l.EmployeeId == employeeId
                && l.StartDate <= request.EndDate
                && l.EndDate >= request.StartDate
                && l.StatusId == (int)LeaveRequestStatusEnum.Pending);

        if (hasOverlap)
            return null;

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DurationDays = duration,
            Reason = request.Reason,
            StatusId = (int)LeaveRequestStatusEnum.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        return await GetFullResponseAsync(leaveRequest.Id);
    }

    public async Task<List<LeaveRequestResponse>> GetMyLeaveRequestsAsync(int employeeId)
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.Reviewer)
            .Include(l => l.Status)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.SubmittedAt)
            .Select(l => MapToResponse(l))
            .ToListAsync();
    }

    public async Task<LeaveRequestResponse?> GetLeaveRequestByIdAsync(int id)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.Reviewer)
            .Include(l => l.Status)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (leaveRequest is null)
            return null;

        var requesterId = leaveRequest.EmployeeId;
        var isOwner = _currentUser.UserId == requesterId;
        var isManager = _currentUser.Role == "Manager"
            && await _context.Users.AnyAsync(u => u.Id == requesterId && u.ManagerId == _currentUser.UserId);
        var isAdmin = _currentUser.Role == "Admin";

        if (!isOwner && !isManager && !isAdmin)
            return null;

        return MapToResponse(leaveRequest);
    }

    public async Task<bool> UpdateLeaveRequestAsync(int id, UpdateLeaveRequest request, int employeeId)
    {
        var leaveRequest = await _context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == id && l.EmployeeId == employeeId);

        if (leaveRequest is null || leaveRequest.StatusId != (int)LeaveRequestStatusEnum.Pending)
            return false;

        var duration = (int)(request.EndDate - request.StartDate).TotalDays + 1;

        if (duration <= 0)
            return false;

        var hasOverlap = await _context.LeaveRequests
            .AnyAsync(l => l.Id != id
                && l.EmployeeId == employeeId
                && l.StartDate <= request.EndDate
                && l.EndDate >= request.StartDate
                && l.StatusId == (int)LeaveRequestStatusEnum.Pending);

        if (hasOverlap)
            return false;

        leaveRequest.LeaveType = request.LeaveType;
        leaveRequest.StartDate = request.StartDate;
        leaveRequest.EndDate = request.EndDate;
        leaveRequest.DurationDays = duration;
        leaveRequest.Reason = request.Reason;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelLeaveRequestAsync(int id, int employeeId)
    {
        var leaveRequest = await _context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == id && l.EmployeeId == employeeId);

        if (leaveRequest is null || leaveRequest.StatusId != (int)LeaveRequestStatusEnum.Pending)
            return false;

        leaveRequest.StatusId = (int)LeaveRequestStatusEnum.Cancelled;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<LeaveRequestResponse>> GetTeamLeaveRequestsAsync(int managerId)
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.Reviewer)
            .Include(l => l.Status)
            .Where(l => l.Employee.ManagerId == managerId)
            .OrderByDescending(l => l.SubmittedAt)
            .Select(l => MapToResponse(l))
            .ToListAsync();
    }

    public async Task<List<LeaveRequestResponse>> GetAllLeaveRequestsAsync()
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.Reviewer)
            .Include(l => l.Status)
            .OrderByDescending(l => l.SubmittedAt)
            .Select(l => MapToResponse(l))
            .ToListAsync();
    }

    public async Task<List<LeaveRequestResponse>> GetPendingLeaveRequestsAsync()
    {
        return await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.Reviewer)
            .Include(l => l.Status)
            .Where(l => l.StatusId == (int)LeaveRequestStatusEnum.Pending)
            .OrderByDescending(l => l.SubmittedAt)
            .Select(l => MapToResponse(l))
            .ToListAsync();
    }

    public async Task<bool> ApproveLeaveRequestAsync(int id, int reviewerId, string? comment)
    {
        var leaveRequest = await _context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == id && l.StatusId == (int)LeaveRequestStatusEnum.Pending);

        if (leaveRequest is null)
            return false;

        leaveRequest.StatusId = (int)LeaveRequestStatusEnum.Approved;
        leaveRequest.ReviewedById = reviewerId;
        leaveRequest.ReviewComment = comment;
        leaveRequest.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectLeaveRequestAsync(int id, int reviewerId, string? comment)
    {
        var leaveRequest = await _context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == id && l.StatusId == (int)LeaveRequestStatusEnum.Pending);

        if (leaveRequest is null)
            return false;

        leaveRequest.StatusId = (int)LeaveRequestStatusEnum.Rejected;
        leaveRequest.ReviewedById = reviewerId;
        leaveRequest.ReviewComment = comment;
        leaveRequest.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<LeaveRequestResponse?> GetFullResponseAsync(int id)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.Reviewer)
            .Include(l => l.Status)
            .FirstOrDefaultAsync(l => l.Id == id);

        return leaveRequest is null ? null : MapToResponse(leaveRequest);
    }

    private static LeaveRequestResponse MapToResponse(LeaveRequest l) => new()
    {
        Id = l.Id,
        EmployeeId = l.EmployeeId,
        EmployeeName = $"{l.Employee.FirstName} {l.Employee.LastName}",
        LeaveType = l.LeaveType,
        StartDate = l.StartDate,
        EndDate = l.EndDate,
        DurationDays = l.DurationDays,
        Reason = l.Reason,
        Status = l.Status.Name,
        ReviewerId = l.ReviewedById,
        ReviewerName = l.Reviewer != null
            ? $"{l.Reviewer.FirstName} {l.Reviewer.LastName}"
            : null,
        ReviewComment = l.ReviewComment,
        SubmittedAt = l.SubmittedAt,
        ReviewedAt = l.ReviewedAt
    };
}
