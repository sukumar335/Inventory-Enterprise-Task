using InventoryEnterpriseProject.Core.Entities;
using InventoryEnterpriseProject.Core.Interfaces;
using InventoryEnterpriseProject.Infrastructure.Data;

namespace InventoryEnterpriseProject.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public User? GetByUsername(string username)
    {
        return _context.Users.FirstOrDefault(u => u.Username == username && !u.IsDeleted);
    }

    public void Add(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }
}
