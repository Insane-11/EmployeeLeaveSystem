using Microsoft.EntityFrameworkCore;
using Moq;
using EmployeeLeaveSystem.Api.Data;
using EmployeeLeaveSystem.Api.Models;
using EmployeeLeaveSystem.Api.Models.DTOs.LeaveRequests;
using EmployeeLeaveSystem.Api.Models.Entities;
using EmployeeLeaveSystem.Api.Models.Enums;
using EmployeeLeaveSystem.Api.Services;

namespace EmployeeLeaveSystem.Api.Tests.Tests;

public class LeaveRequestServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private async Task SeedBasicData(AppDbContext context)
    {
        context.Roles.AddRange(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "Manager" },
            new Role { Id = 3, Name = "Employee" }
        );
        context.LeaveRequestStatuses.AddRange(
            new LeaveRequestStatus { Id = 1, Name = "Pending" },
            new LeaveRequestStatus { Id = 2, Name = "Approved" },
            new LeaveRequestStatus { Id = 3, Name = "Rejected" },
            new LeaveRequestStatus { Id = 4, Name = "Cancelled" }
        );
        context.Users.Add(new User
        {
            Id = 1,
            FirstName = "Employee",
            LastName = "One",
            Email = "emp1@test.com",
            PasswordHash = "hash",
            RoleId = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        context.LeaveBalances.Add(new LeaveBalance
        {
            UserId = 1,
            LeaveType = "Annual",
            Year = DateTime.UtcNow.Year,
            TotalDays = 20,
            UsedDays = 0,
            RemainingDays = 20
        });
        await context.SaveChangesAsync();
    }

    private Mock<ICurrentUserService> CreateCurrentUserMock(int userId = 1, string role = "Employee")
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(m => m.UserId).Returns(userId);
        mock.Setup(m => m.Role).Returns(role);
        return mock;
    }

    [Fact]
    public async Task CreateLeaveRequest_PastStartDate_ReturnsFailure()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var balanceService = new LeaveBalanceService(context);
        var currentUser = CreateCurrentUserMock();
        var service = new LeaveRequestService(context, currentUser.Object, balanceService);

        var result = await service.CreateLeaveRequestAsync(new CreateLeaveRequest
        {
            LeaveType = "Annual",
            StartDate = DateTime.UtcNow.AddDays(-5),
            EndDate = DateTime.UtcNow.AddDays(-3),
            Reason = "Test"
        }, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal("Start date cannot be in the past.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLeaveRequest_EndBeforeStart_ReturnsFailure()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var balanceService = new LeaveBalanceService(context);
        var currentUser = CreateCurrentUserMock();
        var service = new LeaveRequestService(context, currentUser.Object, balanceService);

        var result = await service.CreateLeaveRequestAsync(new CreateLeaveRequest
        {
            LeaveType = "Annual",
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(3),
            Reason = "Test"
        }, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal("End date must be on or after the start date.", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLeaveRequest_ExceedingBalance_ReturnsFailure()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var balanceService = new LeaveBalanceService(context);
        var currentUser = CreateCurrentUserMock();
        var service = new LeaveRequestService(context, currentUser.Object, balanceService);

        var result = await service.CreateLeaveRequestAsync(new CreateLeaveRequest
        {
            LeaveType = "Annual",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(25),
            Reason = "Test"
        }, 1);

        Assert.False(result.IsSuccess);
        Assert.Contains("Insufficient", result.ErrorMessage);
    }

    [Fact]
    public async Task CreateLeaveRequest_OverlappingRequest_ReturnsFailure()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        context.LeaveRequests.Add(new LeaveRequest
        {
            EmployeeId = 1,
            LeaveType = "Annual",
            StartDate = DateTime.UtcNow.AddDays(3),
            EndDate = DateTime.UtcNow.AddDays(7),
            DurationDays = 5,
            StatusId = (int)LeaveRequestStatusEnum.Pending,
            SubmittedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var balanceService = new LeaveBalanceService(context);
        var currentUser = CreateCurrentUserMock();
        var service = new LeaveRequestService(context, currentUser.Object, balanceService);

        var result = await service.CreateLeaveRequestAsync(new CreateLeaveRequest
        {
            LeaveType = "Annual",
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(6),
            Reason = "Test"
        }, 1);

        Assert.False(result.IsSuccess);
        Assert.Contains("overlap", result.ErrorMessage!.ToLower());
    }

    [Fact]
    public async Task CreateLeaveRequest_Valid_CreatesWithPendingStatus()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var balanceService = new LeaveBalanceService(context);
        var currentUser = CreateCurrentUserMock();
        var service = new LeaveRequestService(context, currentUser.Object, balanceService);

        var result = await service.CreateLeaveRequestAsync(new CreateLeaveRequest
        {
            LeaveType = "Annual",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(12),
            Reason = "Vacation"
        }, 1);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Pending", result.Data.Status);
    }

    [Fact]
    public async Task ApproveLeaveRequest_ValidRequest_DeductsBalance()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        context.LeaveRequests.Add(new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveType = "Annual",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(12),
            DurationDays = 3,
            StatusId = (int)LeaveRequestStatusEnum.Pending,
            SubmittedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var balanceService = new LeaveBalanceService(context);
        var currentUser = CreateCurrentUserMock(2, "Admin");
        var service = new LeaveRequestService(context, currentUser.Object, balanceService);

        var result = await service.ApproveLeaveRequestAsync(1, 2, "Approved");

        Assert.True(result);

        var balance = await context.LeaveBalances.FirstAsync(b => b.UserId == 1 && b.LeaveType == "Annual");
        Assert.Equal(3, balance.UsedDays);
        Assert.Equal(17, balance.RemainingDays);
    }

    [Fact]
    public async Task ApproveLeaveRequest_NonPending_ReturnsFalse()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        context.LeaveRequests.Add(new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveType = "Annual",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(12),
            DurationDays = 3,
            StatusId = (int)LeaveRequestStatusEnum.Approved,
            SubmittedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var balanceService = new LeaveBalanceService(context);
        var currentUser = CreateCurrentUserMock(2, "Admin");
        var service = new LeaveRequestService(context, currentUser.Object, balanceService);

        var result = await service.ApproveLeaveRequestAsync(1, 2, null);

        Assert.False(result);
    }

    [Fact]
    public async Task CancelLeaveRequest_Pending_Cancels()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        context.LeaveRequests.Add(new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveType = "Annual",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(12),
            DurationDays = 3,
            StatusId = (int)LeaveRequestStatusEnum.Pending,
            SubmittedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var balanceService = new LeaveBalanceService(context);
        var currentUser = CreateCurrentUserMock();
        var service = new LeaveRequestService(context, currentUser.Object, balanceService);

        var result = await service.CancelLeaveRequestAsync(1, 1);

        Assert.True(result);

        var request = await context.LeaveRequests.FindAsync(1);
        Assert.Equal((int)LeaveRequestStatusEnum.Cancelled, request!.StatusId);
    }

    [Fact]
    public async Task CancelLeaveRequest_Approved_ReturnsFalse()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        context.LeaveRequests.Add(new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveType = "Annual",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(12),
            DurationDays = 3,
            StatusId = (int)LeaveRequestStatusEnum.Approved,
            SubmittedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var balanceService = new LeaveBalanceService(context);
        var currentUser = CreateCurrentUserMock();
        var service = new LeaveRequestService(context, currentUser.Object, balanceService);

        var result = await service.CancelLeaveRequestAsync(1, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task GetMyLeaveRequests_ReturnsOnlyOwnRequests()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        context.Users.Add(new User
        {
            Id = 2,
            FirstName = "Other",
            LastName = "User",
            Email = "other@test.com",
            PasswordHash = "hash",
            RoleId = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        context.LeaveRequests.AddRange(
            new LeaveRequest
            {
                EmployeeId = 1,
                LeaveType = "Annual",
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(12),
                DurationDays = 3,
                StatusId = (int)LeaveRequestStatusEnum.Pending,
                SubmittedAt = DateTime.UtcNow.AddDays(-1)
            },
            new LeaveRequest
            {
                EmployeeId = 2,
                LeaveType = "Sick",
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(6),
                DurationDays = 2,
                StatusId = (int)LeaveRequestStatusEnum.Pending,
                SubmittedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        var balanceService = new LeaveBalanceService(context);
        var currentUser = CreateCurrentUserMock();
        var service = new LeaveRequestService(context, currentUser.Object, balanceService);

        var results = await service.GetMyLeaveRequestsAsync(1);

        Assert.Single(results);
        Assert.Equal(1, results[0].EmployeeId);
    }
}
