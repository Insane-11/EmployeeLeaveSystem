using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeLeaveSystem.Api.Models.DTOs.LeaveRequests;
using EmployeeLeaveSystem.Api.Services;

namespace EmployeeLeaveSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly ICurrentUserService _currentUser;

    public LeaveRequestsController(
        ILeaveRequestService leaveRequestService,
        ICurrentUserService currentUser)
    {
        _leaveRequestService = leaveRequestService;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeaveRequest request)
    {
        var result = await _leaveRequestService.CreateLeaveRequestAsync(request, _currentUser.UserId);

        if (result is null)
            return BadRequest(new { message = "Invalid request. Check dates or overlapping leave." });

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyRequests()
    {
        var requests = await _leaveRequestService.GetMyLeaveRequestsAsync(_currentUser.UserId);
        return Ok(requests);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _leaveRequestService.GetLeaveRequestByIdAsync(id);

        if (result is null)
            return Forbid();

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLeaveRequest request)
    {
        var result = await _leaveRequestService.UpdateLeaveRequestAsync(id, request, _currentUser.UserId);

        if (!result)
            return BadRequest(new { message = "Unable to update. Leave request may not exist, is not pending, or has date conflicts." });

        return Ok(new { message = "Leave request updated successfully" });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _leaveRequestService.CancelLeaveRequestAsync(id, _currentUser.UserId);

        if (!result)
            return BadRequest(new { message = "Unable to cancel. Leave request may not exist or is not pending." });

        return Ok(new { message = "Leave request cancelled successfully" });
    }

    [HttpGet("team")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> GetTeamRequests()
    {
        var requests = await _leaveRequestService.GetTeamLeaveRequestsAsync(_currentUser.UserId);
        return Ok(requests);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var requests = await _leaveRequestService.GetAllLeaveRequestsAsync();
        return Ok(requests);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending()
    {
        var requests = await _leaveRequestService.GetPendingLeaveRequestsAsync();
        return Ok(requests);
    }

    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, [FromBody] ReviewLeaveRequest review)
    {
        var result = await _leaveRequestService.ApproveLeaveRequestAsync(id, _currentUser.UserId, review.ReviewComment);

        if (!result)
            return NotFound(new { message = "Leave request not found or already reviewed." });

        return Ok(new { message = "Leave request approved" });
    }

    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(int id, [FromBody] ReviewLeaveRequest review)
    {
        var result = await _leaveRequestService.RejectLeaveRequestAsync(id, _currentUser.UserId, review.ReviewComment);

        if (!result)
            return NotFound(new { message = "Leave request not found or already reviewed." });

        return Ok(new { message = "Leave request rejected" });
    }
}
