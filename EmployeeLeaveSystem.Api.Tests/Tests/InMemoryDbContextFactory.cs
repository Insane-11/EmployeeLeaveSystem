using Microsoft.EntityFrameworkCore;
using EmployeeLeaveSystem.Api.Data;

namespace EmployeeLeaveSystem.Api.Tests.Tests;

public static class InMemoryDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
