using Microsoft.EntityFrameworkCore;
using EmployeeLeaveSystem.Api.Data;
using EmployeeLeaveSystem.Api.Models.Entities;

namespace EmployeeLeaveSystem.Api.Services;

public class LeaveBalanceService : ILeaveBalanceService
{
    private readonly AppDbContext _context;

    private static readonly Dictionary<string, decimal> DefaultTotals = new()
    {
        { "Annual", 20m },
        { "Sick", 10m },
        { "Personal", 5m },
        { "Maternity", 90m },
        { "Paternity", 10m },
        { "Unpaid", 0m }
    };

    public LeaveBalanceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LeaveBalance> GetOrCreateBalanceAsync(int userId, string leaveType, int year)
    {
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b => b.UserId == userId
                                   && b.LeaveType == leaveType
                                   && b.Year == year);

        if (balance is not null)
            return balance;

        var totalDays = DefaultTotals.GetValueOrDefault(leaveType, 0m);

        balance = new LeaveBalance
        {
            UserId = userId,
            LeaveType = leaveType,
            Year = year,
            TotalDays = totalDays,
            UsedDays = 0,
            RemainingDays = totalDays
        };

        _context.LeaveBalances.Add(balance);
        await _context.SaveChangesAsync();

        return balance;
    }

    public async Task<bool> DeductBalanceAsync(int userId, string leaveType, decimal days, int year)
    {
        var balance = await GetOrCreateBalanceAsync(userId, leaveType, year);

        if (balance.RemainingDays < days)
            return false;

        balance.UsedDays += days;
        balance.RemainingDays = balance.TotalDays - balance.UsedDays;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetRemainingDaysAsync(int userId, string leaveType, int year)
    {
        var balance = await GetOrCreateBalanceAsync(userId, leaveType, year);
        return balance.RemainingDays;
    }

    public async Task<List<LeaveBalance>> GetUserBalancesAsync(int userId, int? year)
    {
        var query = _context.LeaveBalances
            .Where(b => b.UserId == userId);

        if (year.HasValue)
            query = query.Where(b => b.Year == year.Value);

        var balances = await query.ToListAsync();
        var existingKeys = balances.Select(b => b.LeaveType).ToHashSet();

        var currentYear = year ?? DateTime.UtcNow.Year;

        foreach (var kvp in DefaultTotals)
        {
            if (!existingKeys.Contains(kvp.Key))
            {
                balances.Add(new LeaveBalance
                {
                    UserId = userId,
                    LeaveType = kvp.Key,
                    Year = currentYear,
                    TotalDays = kvp.Value,
                    UsedDays = 0,
                    RemainingDays = kvp.Value
                });
            }
        }

        return balances.OrderBy(b => b.LeaveType).ToList();
    }
}
