using InventoryEnterpriseProject.Core.Entities;

namespace InventoryEnterpriseProject.Core.Interfaces;

public interface IUserRepository
{
    User? GetByUsername(string username);
    void Add(User user);
}
