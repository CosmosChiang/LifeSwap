using LifeSwap.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LifeSwap.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<TimeOffRequest> TimeOffRequests => Set<TimeOffRequest>();

    public DbSet<AppNotification> Notifications => Set<AppNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.Property(user => user.EmployeeId).HasMaxLength(64).IsRequired();
            entity.Property(user => user.Username).HasMaxLength(128).IsRequired();
            entity.Property(user => user.PasswordHash).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(256).IsRequired();
            entity.Property(user => user.DepartmentCode).HasMaxLength(32).IsRequired();
            entity.HasMany(user => user.UserRoles)
                .WithOne(userRole => userRole.User)
                .HasForeignKey(userRole => userRole.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(role => role.Id);
            entity.Property(role => role.Name).HasMaxLength(128).IsRequired();
            entity.Property(role => role.Description).HasMaxLength(512);
            entity.HasMany(role => role.UserRoles)
                .WithOne(userRole => userRole.Role)
                .HasForeignKey(userRole => userRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
        });

        modelBuilder.Entity<TimeOffRequest>(entity =>
        {
            entity.HasKey(request => request.Id);
            entity.Property(request => request.EmployeeId).HasMaxLength(64).IsRequired();
            entity.Property(request => request.DepartmentCode).HasMaxLength(32).IsRequired();
            entity.Property(request => request.Reason).HasMaxLength(512).IsRequired();
            entity.Property(request => request.ReviewComment).HasMaxLength(512);
            entity.Property(request => request.ReviewerId).HasMaxLength(64);
        });

        modelBuilder.Entity<AppNotification>(entity =>
        {
            entity.HasKey(notification => notification.Id);
            entity.Property(notification => notification.RecipientEmployeeId).HasMaxLength(64).IsRequired();
            entity.Property(notification => notification.Title).HasMaxLength(128).IsRequired();
            entity.Property(notification => notification.Message).HasMaxLength(512).IsRequired();
        });
    }
}