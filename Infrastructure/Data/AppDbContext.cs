
using Microsoft.EntityFrameworkCore;
using InventoryEnterpriseProject.Core.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InventoryEnterpriseProject.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<User> Users => Set<User>();

    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = currentUser;
            }

            if (entry.State == EntityState.Modified)
            {
                // If it is newly marked as deleted
                if (entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
                {
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.DeletedBy = currentUser;
                }
                else
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = currentUser;
                }
            }
        }

        return base.SaveChanges();
    }
}
