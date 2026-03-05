using System.Security.Cryptography;
using System.Text;
using InventoryEnterpriseProject.Core.Entities;
using InventoryEnterpriseProject.Core.Interfaces;

namespace InventoryEnterpriseProject.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public User? Authenticate(string username, string password)
    {
        var user = _repository.GetByUsername(username);
        if (user == null)
            return null;

        var hash = HashPassword(password);
        if (user.PasswordHash != hash)
            return null;

        return user;
    }

    public User? Register(string username, string email, string firstName, string lastName, string password)
    {
        if (_repository.GetByUsername(username) != null)
            return null; // User already exists

        var user = new User
        {
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = HashPassword(password)
        };

        _repository.Add(user);
        return user;
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
