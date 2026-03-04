
using InventoryEnterpriseProject.Core.Entities;
using InventoryEnterpriseProject.Core.Interfaces;
using InventoryEnterpriseProject.Infrastructure.Data;

namespace InventoryEnterpriseProject.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _context;

    public InventoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<InventoryItem> GetAll()
    {
        return _context.InventoryItems.Where(x => !x.IsDeleted).ToList();
    }

    public void Add(InventoryItem item)
    {
        _context.InventoryItems.Add(item);
        _context.SaveChanges();
    }

    public InventoryItem? GetById(int id)
    {
        return _context.InventoryItems.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
    }

    public void Update(InventoryItem item)
    {
        _context.InventoryItems.Update(item);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var item = _context.InventoryItems.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            item.IsDeleted = true;
            _context.SaveChanges();
        }
    }
}
