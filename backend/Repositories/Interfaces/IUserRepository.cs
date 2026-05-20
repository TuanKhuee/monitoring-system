using backend.Model;

namespace backend.Repositories.Interfaces;

public interface IUserRepository : IMongoRepository<User>
{
    Task<User> GetByUsernameAsync(string username);
}