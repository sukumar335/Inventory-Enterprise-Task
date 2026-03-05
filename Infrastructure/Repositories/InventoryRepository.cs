
using InventoryEnterpriseProject.Core.Entities;
using InventoryEnterpriseProject.Core.Interfaces;
using InventoryEnterpriseProject.Infrastructure.Data;

namespace InventoryEnterpriseProject.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _context;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

    public InventoryRepository(AppDbContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
    }

    public List<InventoryItem> GetAll()
    {
        var currentUser = GetCurrentUser();
        return _context.InventoryItems.Where(x => !x.IsDeleted && x.CreatedBy == currentUser).ToList();
    }

    public void Add(InventoryItem item)
    {
        _context.InventoryItems.Add(item);
        _context.SaveChanges();
    }

    public InventoryItem? GetById(int id)
    {
        var currentUser = GetCurrentUser();
        return _context.InventoryItems.FirstOrDefault(x => x.Id == id && !x.IsDeleted && x.CreatedBy == currentUser);
    }

    public void Update(InventoryItem item)
    {
        var currentUser = GetCurrentUser();
        var existingItem = _context.InventoryItems.FirstOrDefault(x => x.Id == item.Id && !x.IsDeleted && x.CreatedBy == currentUser);
        if (existingItem != null)
        {
            existingItem.Name = item.Name;
            existingItem.SKU = item.SKU;
            existingItem.Category = item.Category;
            existingItem.Price = item.Price;
            existingItem.Quantity = item.Quantity;
            _context.SaveChanges();
        }
    }

    public void Delete(int id)
    {
        var currentUser = GetCurrentUser();
        var item = _context.InventoryItems.FirstOrDefault(x => x.Id == id && !x.IsDeleted && x.CreatedBy == currentUser);
        if (item != null)
        {
            item.IsDeleted = true;
            _context.SaveChanges();
        }
    }
}
