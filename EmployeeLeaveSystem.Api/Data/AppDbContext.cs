using Microsoft.EntityFrameworkCore;
using EmployeeLeaveSystem.Api.Models.Entities;

namespace EmployeeLeaveSystem.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<LeaveRequestStatus> LeaveRequestStatuses => Set<LeaveRequestStatus>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(r => r.Name).IsUnique();
            entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Description).HasMaxLength(255);
        });

        // LeaveRequestStatus
        modelBuilder.Entity<LeaveRequestStatus>(entity =>
        {
            entity.HasIndex(s => s.Name).IsUnique();
            entity.Property(s => s.Name).IsRequired().HasMaxLength(50);
        });

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(u => u.Department).HasMaxLength(100);
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(u => u.Role)
                  .WithMany(r => r.Users)
                  .HasForeignKey(u => u.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.Manager)
                  .WithMany(u => u.Subordinates)
                  .HasForeignKey(u => u.ManagerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // LeaveRequest
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.Property(l => l.LeaveType).IsRequired().HasMaxLength(50);
            entity.Property(l => l.Reason).HasMaxLength(1000);
            entity.Property(l => l.ReviewComment).HasMaxLength(500);
            entity.Property(l => l.DurationDays).HasColumnType("decimal(4,1)");
            entity.Property(l => l.SubmittedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(l => l.Employee)
                  .WithMany(u => u.LeaveRequests)
                  .HasForeignKey(l => l.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Reviewer)
                  .WithMany(u => u.ReviewedRequests)
                  .HasForeignKey(l => l.ReviewedById)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Status)
                  .WithMany(s => s.LeaveRequests)
                  .HasForeignKey(l => l.StatusId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // LeaveBalance
        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.HasIndex(l => new { l.UserId, l.LeaveType, l.Year }).IsUnique();
            entity.Property(l => l.LeaveType).IsRequired().HasMaxLength(50);
            entity.Property(l => l.TotalDays).HasColumnType("decimal(4,1)");
            entity.Property(l => l.UsedDays).HasColumnType("decimal(4,1)");
            entity.Property(l => l.RemainingDays).HasColumnType("decimal(4,1)");

            entity.HasOne(l => l.User)
                  .WithMany(u => u.LeaveBalances)
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "System administrator" },
            new Role { Id = 2, Name = "Manager", Description = "Team manager who can review requests" },
            new Role { Id = 3, Name = "Employee", Description = "Regular employee who can submit requests" }
        );

        modelBuilder.Entity<LeaveRequestStatus>().HasData(
            new LeaveRequestStatus { Id = 1, Name = "Pending" },
            new LeaveRequestStatus { Id = 2, Name = "Approved" },
            new LeaveRequestStatus { Id = 3, Name = "Rejected" },
            new LeaveRequestStatus { Id = 4, Name = "Cancelled" }
        );
    }
}
