using Microsoft.EntityFrameworkCore;
using Moq;
using EmployeeLeaveSystem.Api.Data;
using EmployeeLeaveSystem.Api.Models.DTOs.Users;
using EmployeeLeaveSystem.Api.Models.Entities;
using EmployeeLeaveSystem.Api.Services;

namespace EmployeeLeaveSystem.Api.Tests.Tests;

public class UserServiceTests
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
        context.Users.AddRange(
            new User
            {
                Id = 1,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@test.com",
                PasswordHash = "hash",
                RoleId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 2,
                FirstName = "Manager",
                LastName = "User",
                Email = "mgr@test.com",
                PasswordHash = "hash",
                RoleId = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 3,
                FirstName = "Employee",
                LastName = "One",
                Email = "emp1@test.com",
                PasswordHash = "hash",
                RoleId = 3,
                IsActive = true,
                ManagerId = 2,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = 4,
                FirstName = "Inactive",
                LastName = "User",
                Email = "inactive@test.com",
                PasswordHash = "hash",
                RoleId = 3,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllUsersAsync_Admin_ReturnsAllUsers()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(m => m.Role).Returns("Admin");

        var service = new UserService(context, currentUser.Object);
        var results = await service.GetAllUsersAsync();

        Assert.Equal(4, results.Count);
    }

    [Fact]
    public async Task GetUserByIdAsync_Admin_CanViewAnyUser()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(m => m.Role).Returns("Admin");
        currentUser.Setup(m => m.UserId).Returns(1);

        var service = new UserService(context, currentUser.Object);
        var result = await service.GetUserByIdAsync(3);

        Assert.NotNull(result);
        Assert.Equal("Employee One", result.FullName);
    }

    [Fact]
    public async Task GetUserByIdAsync_User_CanViewSelf()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(m => m.Role).Returns("Employee");
        currentUser.Setup(m => m.UserId).Returns(3);

        var service = new UserService(context, currentUser.Object);
        var result = await service.GetUserByIdAsync(3);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetUserByIdAsync_User_CannotViewOtherNonAdmin()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(m => m.Role).Returns("Employee");
        currentUser.Setup(m => m.UserId).Returns(3);

        var service = new UserService(context, currentUser.Object);
        var result = await service.GetUserByIdAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserAsync_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(m => m.Role).Returns("Admin");

        var service = new UserService(context, currentUser.Object);
        var result = await service.UpdateUserAsync(3, new UpdateUserRequest
        {
            FirstName = "Updated"
        });

        Assert.True(result);
        var user = await context.Users.FindAsync(3);
        Assert.Equal("Updated", user!.FirstName);
        Assert.Equal("One", user.LastName);
    }

    [Fact]
    public async Task UpdateUserAsync_NonExistent_ReturnsFalse()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(m => m.Role).Returns("Admin");

        var service = new UserService(context, currentUser.Object);
        var result = await service.UpdateUserAsync(999, new UpdateUserRequest
        {
            FirstName = "Ghost"
        });

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteUserAsync_ExistingUser_RemovesUser()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(m => m.Role).Returns("Admin");

        var service = new UserService(context, currentUser.Object);
        var result = await service.DeleteUserAsync(3);

        Assert.True(result);
        Assert.Null(await context.Users.FindAsync(3));
    }

    [Fact]
    public async Task DeleteUserAsync_NonExistent_ReturnsFalse()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(m => m.Role).Returns("Admin");

        var service = new UserService(context, currentUser.Object);
        var result = await service.DeleteUserAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task GetTeamMembersAsync_ReturnsDirectReports()
    {
        var context = CreateContext();
        await SeedBasicData(context);
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(m => m.Role).Returns("Manager");
        currentUser.Setup(m => m.UserId).Returns(2);

        var service = new UserService(context, currentUser.Object);
        var results = await service.GetTeamMembersAsync();

        Assert.Single(results);
        Assert.Equal("Employee One", results[0].FullName);
    }
}
