using Microsoft.EntityFrameworkCore;
using EmployeeLeaveSystem.Api.Data;
using EmployeeLeaveSystem.Api.Models.Entities;
using EmployeeLeaveSystem.Api.Services;

namespace EmployeeLeaveSystem.Api.Tests.Tests;

public class LeaveBalanceServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetOrCreateBalanceAsync_Existing_ReturnsExisting()
    {
        var context = CreateContext();
        context.LeaveBalances.Add(new LeaveBalance
        {
            UserId = 1,
            LeaveType = "Annual",
            Year = 2026,
            TotalDays = 20,
            UsedDays = 5,
            RemainingDays = 15
        });
        await context.SaveChangesAsync();

        var service = new LeaveBalanceService(context);
        var balance = await service.GetOrCreateBalanceAsync(1, "Annual", 2026);

        Assert.Equal(15, balance.RemainingDays);
        Assert.Equal(5, balance.UsedDays);
    }

    [Fact]
    public async Task GetOrCreateBalanceAsync_New_CreatesWithDefaults()
    {
        var context = CreateContext();
        var service = new LeaveBalanceService(context);
        var balance = await service.GetOrCreateBalanceAsync(1, "Annual", 2026);

        Assert.NotNull(balance);
        Assert.Equal(20, balance.TotalDays);
        Assert.Equal(0, balance.UsedDays);
        Assert.Equal(20, balance.RemainingDays);
    }

    [Fact]
    public async Task DeductBalanceAsync_SufficientBalance_Deducts()
    {
        var context = CreateContext();
        var service = new LeaveBalanceService(context);
        var balance = await service.GetOrCreateBalanceAsync(1, "Annual", 2026);
        Assert.Equal(20, balance.RemainingDays);

        var result = await service.DeductBalanceAsync(1, "Annual", 5, 2026);

        Assert.True(result);
        var updated = await context.LeaveBalances
            .FirstAsync(b => b.UserId == 1 && b.LeaveType == "Annual");
        Assert.Equal(5, updated.UsedDays);
        Assert.Equal(15, updated.RemainingDays);
    }

    [Fact]
    public async Task DeductBalanceAsync_InsufficientBalance_ReturnsFalse()
    {
        var context = CreateContext();
        var service = new LeaveBalanceService(context);
        await service.GetOrCreateBalanceAsync(1, "Personal", 2026);

        var result = await service.DeductBalanceAsync(1, "Personal", 10, 2026);

        Assert.False(result);
    }

    [Fact]
    public async Task DeductBalanceAsync_MultipleDeductions_Accumulates()
    {
        var context = CreateContext();
        var service = new LeaveBalanceService(context);
        await service.GetOrCreateBalanceAsync(1, "Annual", 2026);

        await service.DeductBalanceAsync(1, "Annual", 3, 2026);
        await service.DeductBalanceAsync(1, "Annual", 4, 2026);

        var updated = await context.LeaveBalances
            .FirstAsync(b => b.UserId == 1 && b.LeaveType == "Annual");
        Assert.Equal(7, updated.UsedDays);
        Assert.Equal(13, updated.RemainingDays);
    }

    [Fact]
    public async Task GetRemainingDaysAsync_ReturnsCorrectValue()
    {
        var context = CreateContext();
        var service = new LeaveBalanceService(context);
        await service.GetOrCreateBalanceAsync(1, "Sick", 2026);
        await service.DeductBalanceAsync(1, "Sick", 3, 2026);

        var remaining = await service.GetRemainingDaysAsync(1, "Sick", 2026);

        Assert.Equal(7, remaining);
    }

    [Fact]
    public async Task GetUserBalancesAsync_ReturnsAllTypes()
    {
        var context = CreateContext();
        context.LeaveBalances.Add(new LeaveBalance
        {
            UserId = 1,
            LeaveType = "Annual",
            Year = 2026,
            TotalDays = 20,
            UsedDays = 0,
            RemainingDays = 20
        });
        await context.SaveChangesAsync();

        var service = new LeaveBalanceService(context);
        var balances = await service.GetUserBalancesAsync(1, 2026);

        Assert.Equal(6, balances.Count);
        Assert.Contains(balances, b => b.LeaveType == "Annual");
        Assert.Contains(balances, b => b.LeaveType == "Sick");
        Assert.Contains(balances, b => b.LeaveType == "Personal");
    }
}
