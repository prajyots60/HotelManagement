using UserService.Models;

namespace UserService.Repositories.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();

    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByUsernameAsync(string username);

    Task<bool> UsernameExistsAsync(string username);

    Task<User> AddAsync(User user);

    Task UpdateAsync(User user);

    Task DeleteAsync(User user);
}
