using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeLeaveSystem.Api.Models.DTOs;
using EmployeeLeaveSystem.Api.Services;

namespace EmployeeLeaveSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveBalancesController : ControllerBase
{
    private readonly ILeaveBalanceService _leaveBalanceService;
    private readonly ICurrentUserService _currentUser;

    public LeaveBalancesController(
        ILeaveBalanceService leaveBalanceService,
        ICurrentUserService currentUser)
    {
        _leaveBalanceService = leaveBalanceService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyBalance([FromQuery] int? year)
    {
        var balances = await _leaveBalanceService.GetUserBalancesAsync(
            _currentUser.UserId, year ?? DateTime.UtcNow.Year);

        var response = balances.Select(b => new LeaveBalanceResponse
        {
            LeaveType = b.LeaveType,
            TotalDays = b.TotalDays,
            UsedDays = b.UsedDays,
            RemainingDays = b.RemainingDays,
            Year = b.Year
        });

        return Ok(response);
    }

    [HttpGet("{userId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserBalance(int userId, [FromQuery] int? year)
    {
        var balances = await _leaveBalanceService.GetUserBalancesAsync(
            userId, year ?? DateTime.UtcNow.Year);

        var response = balances.Select(b => new LeaveBalanceResponse
        {
            LeaveType = b.LeaveType,
            TotalDays = b.TotalDays,
            UsedDays = b.UsedDays,
            RemainingDays = b.RemainingDays,
            Year = b.Year
        });

        return Ok(response);
    }
}
