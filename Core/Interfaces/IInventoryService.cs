
using InventoryEnterpriseProject.Core.Entities;

namespace InventoryEnterpriseProject.Core.Interfaces;

public interface IInventoryService
{
    List<InventoryItem> GetItems();
    InventoryItem? GetItem(int id);
    void CreateItem(InventoryItem item);
    void UpdateItem(InventoryItem item);
    void DeleteItem(int id);
}
