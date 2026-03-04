using InventoryEnterpriseProject.Core.Entities;

namespace InventoryEnterpriseProject.Core.Interfaces;

public interface IUserService
{
    User? Authenticate(string username, string password);
    User? Register(string username, string password);
}
