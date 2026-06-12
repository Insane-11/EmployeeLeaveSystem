using Microsoft.EntityFrameworkCore;
using EmployeeLeaveSystem.Api.Data;
using EmployeeLeaveSystem.Api.Models;
using EmployeeLeaveSystem.Api.Models.DTOs.LeaveRequests;
using EmployeeLeaveSystem.Api.Models.Entities;
using EmployeeLeaveSystem.Api.Models.Enums;

namespace EmployeeLeaveSystem.Api.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILeaveBalanceService _leaveBalanceService;

    public LeaveRequestService(AppDbContext context, ICurrentUserService currentUser, ILeaveBalanceService leaveBalanceService)
    {
        _context = context;
        _currentUser = currentUser;
        _leaveBalanceService = leaveBalanceService;
    }

    public async Task<ServiceResult<LeaveRequestResponse>> CreateLeaveRequestAsync(CreateLeaveRequest request, int employeeId)
    {
        if (request.StartDate.Date < DateTime.UtcNow.Date)
            return ServiceResult<LeaveRequestResponse>.Failure("Start date cannot be in the past.");

        if (request.EndDate < request.StartDate)
            return ServiceResult<LeaveRequestResponse>.Failure("End date must be on or after the start date.");

        var duration = (int)(request.EndDate - request.StartDate).TotalDays + 1;

        if (duration <= 0)
            return ServiceResult<LeaveRequestResponse>.Failure("Duration must be at least 1 day.");

        var currentYear = DateTime.UtcNow.Year;
        var remaining = await _leaveBalanceService.GetRemainingDaysAsync(employeeId, request.LeaveType, currentYear);

        if (duration > remaining)
            return ServiceResult<LeaveRequestResponse>.Failure(
                $"Insufficient {request.LeaveType} leave balance. You have {remaining} day(s) remaining but requested {duration} day(s).");

        var activeStatuses = new[] { (int)LeaveRequestStatusEnum.Pending, (int)LeaveRequestStatusEnum.Approved };

        var hasOverlap = await _context.LeaveRequests
            .AnyAsync(l => l.EmployeeId == employeeId
                && l.StartDate <= request.EndDate
                && l.EndDate >= request.StartDate
                && activeStatuses.Contains(l.StatusId));

        if (hasOverlap)
            return ServiceResult<LeaveRequestResponse>.Failure(
                "You already have a leave request that overlaps with the selected dates.");

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

        var response = await GetFullResponseAsync(leaveRequest.Id);
        return ServiceResult<LeaveRequestResponse>.Success(response!);
    }

    public async Task<List<LeaveRequestResponse>> GetMyLeaveRequestsAsync(int employeeId)
    {
        var requests = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.Reviewer)
            .Include(l => l.Status)
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.SubmittedAt)
            .ToListAsync();
        return requests.Select(l => MapToResponse(l)).ToList();
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

    public async Task<ServiceResult<bool>> UpdateLeaveRequestAsync(int id, UpdateLeaveRequest request, int employeeId)
    {
        var leaveRequest = await _context.LeaveRequests
            .FirstOrDefaultAsync(l => l.Id == id && l.EmployeeId == employeeId);

        if (leaveRequest is null || leaveRequest.StatusId != (int)LeaveRequestStatusEnum.Pending)
            return ServiceResult<bool>.Failure("Leave request not found or is no longer pending.");

        if (request.StartDate.Date < DateTime.UtcNow.Date)
            return ServiceResult<bool>.Failure("Start date cannot be in the past.");

        if (request.EndDate < request.StartDate)
            return ServiceResult<bool>.Failure("End date must be on or after the start date.");

        var duration = (int)(request.EndDate - request.StartDate).TotalDays + 1;

        if (duration <= 0)
            return ServiceResult<bool>.Failure("Duration must be at least 1 day.");

        var currentYear = DateTime.UtcNow.Year;
        var remaining = await _leaveBalanceService.GetRemainingDaysAsync(employeeId, request.LeaveType, currentYear);

        if (duration > remaining)
            return ServiceResult<bool>.Failure(
                $"Insufficient {request.LeaveType} leave balance. You have {remaining} day(s) remaining but requested {duration} day(s).");

        var activeStatuses = new[] { (int)LeaveRequestStatusEnum.Pending, (int)LeaveRequestStatusEnum.Approved };

        var hasOverlap = await _context.LeaveRequests
            .AnyAsync(l => l.Id != id
                && l.EmployeeId == employeeId
                && l.StartDate <= request.EndDate
                && l.EndDate >= request.StartDate
                && activeStatuses.Contains(l.StatusId));

        if (hasOverlap)
            return ServiceResult<bool>.Failure("You already have a leave request that overlaps with the selected dates.");

        leaveRequest.LeaveType = request.LeaveType;
        leaveRequest.StartDate = request.StartDate;
        leaveRequest.EndDate = request.EndDate;
        leaveRequest.DurationDays = duration;
        leaveRequest.Reason = request.Reason;

        await _context.SaveChangesAsync();
        return ServiceResult<bool>.Success(true);
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
        var requests = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.Reviewer)
            .Include(l => l.Status)
            .Where(l => l.Employee.ManagerId == managerId)
            .OrderByDescending(l => l.SubmittedAt)
            .ToListAsync();
        return requests.Select(l => MapToResponse(l)).ToList();
    }

    public async Task<List<LeaveRequestResponse>> GetAllLeaveRequestsAsync()
    {
        var requests = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.Reviewer)
            .Include(l => l.Status)
            .OrderByDescending(l => l.SubmittedAt)
            .ToListAsync();
        return requests.Select(l => MapToResponse(l)).ToList();
    }

    public async Task<List<LeaveRequestResponse>> GetPendingLeaveRequestsAsync()
    {
        var requests = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.Reviewer)
            .Include(l => l.Status)
            .Where(l => l.StatusId == (int)LeaveRequestStatusEnum.Pending)
            .OrderByDescending(l => l.SubmittedAt)
            .ToListAsync();
        return requests.Select(l => MapToResponse(l)).ToList();
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

        await _leaveBalanceService.DeductBalanceAsync(
            leaveRequest.EmployeeId,
            leaveRequest.LeaveType,
            leaveRequest.DurationDays,
            leaveRequest.SubmittedAt.Year);

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
