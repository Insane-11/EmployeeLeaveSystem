using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EmployeeLeaveSystem.Api.Data;
using EmployeeLeaveSystem.Api.Models.DTOs.Auth;
using EmployeeLeaveSystem.Api.Models.Entities;
using EmployeeLeaveSystem.Api.Services;

namespace EmployeeLeaveSystem.Api.Tests.Tests;

public class AuthServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private IConfiguration CreateJwtConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestSecretKey_AtLeast32CharactersLong!!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiryInMinutes"] = "60"
            })!
            .Build();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsLoginResponse()
    {
        var context = CreateContext();
        context.Roles.Add(new Role { Id = 1, Name = "Employee" });
        context.Users.Add(new User
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            RoleId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateJwtConfig());
        var result = await service.Login(new LoginRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        });

        Assert.NotNull(result);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("Employee", result.Role);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("User", result.LastName);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsNull()
    {
        var context = CreateContext();
        context.Roles.Add(new Role { Id = 1, Name = "Employee" });
        context.Users.Add(new User
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            RoleId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateJwtConfig());
        var result = await service.Login(new LoginRequest
        {
            Email = "test@test.com",
            Password = "WrongPassword"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_InactiveUser_ReturnsNull()
    {
        var context = CreateContext();
        context.Roles.Add(new Role { Id = 1, Name = "Employee" });
        context.Users.Add(new User
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            RoleId = 1,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateJwtConfig());
        var result = await service.Login(new LoginRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_NonExistentEmail_ReturnsNull()
    {
        var context = CreateContext();
        var service = new AuthService(context, CreateJwtConfig());
        var result = await service.Login(new LoginRequest
        {
            Email = "nonexistent@test.com",
            Password = "Password123!"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task Register_NewUser_CreatesSuccessfully()
    {
        var context = CreateContext();
        context.Roles.Add(new Role { Id = 3, Name = "Employee" });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateJwtConfig());
        var result = await service.Register(new RegisterRequest
        {
            FirstName = "New",
            LastName = "User",
            Email = "new@test.com",
            Password = "Password123!"
        });

        Assert.True(result);

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "new@test.com");
        Assert.NotNull(user);
        Assert.Equal("New", user.FirstName);
        Assert.Equal("User", user.LastName);
        Assert.Equal(3, user.RoleId);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsFalse()
    {
        var context = CreateContext();
        context.Roles.Add(new Role { Id = 3, Name = "Employee" });
        context.Users.Add(new User
        {
            Id = 1,
            FirstName = "Existing",
            LastName = "User",
            Email = "existing@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            RoleId = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateJwtConfig());
        var result = await service.Register(new RegisterRequest
        {
            FirstName = "Another",
            LastName = "User",
            Email = "existing@test.com",
            Password = "Password123!"
        });

        Assert.False(result);
    }

    [Fact]
    public async Task Register_DefaultRoleIsEmployee()
    {
        var context = CreateContext();
        context.Roles.Add(new Role { Id = 3, Name = "Employee" });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateJwtConfig());
        await service.Register(new RegisterRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            Password = "Password123!"
        });

        var user = await context.Users.FirstAsync(u => u.Email == "test@test.com");
        Assert.Equal(3, user.RoleId);
    }
}
