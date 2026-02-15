using LifeSwap.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LifeSwap.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TimeOffRequest> TimeOffRequests => Set<TimeOffRequest>();

    public DbSet<AppNotification> Notifications => Set<AppNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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