
using InventoryEnterpriseProject.Core.Entities;
using InventoryEnterpriseProject.Core.Interfaces;

namespace InventoryEnterpriseProject.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;

    public InventoryService(IInventoryRepository repository)
    {
        _repository = repository;
    }

    public List<InventoryItem> GetItems() => _repository.GetAll();

    public InventoryItem? GetItem(int id) => _repository.GetById(id);

    public void CreateItem(InventoryItem item) => _repository.Add(item);

    public void UpdateItem(InventoryItem item) => _repository.Update(item);

    public void DeleteItem(int id) => _repository.Delete(id);
}
