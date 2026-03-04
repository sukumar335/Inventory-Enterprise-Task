
using InventoryEnterpriseProject.Core.Entities;

namespace InventoryEnterpriseProject.Core.Interfaces;

public interface IInventoryRepository
{
    List<InventoryItem> GetAll();
    InventoryItem? GetById(int id);
    void Add(InventoryItem item);
    void Update(InventoryItem item);
    void Delete(int id);
}
