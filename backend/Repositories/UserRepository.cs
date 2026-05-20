using backend.Model;
using backend.Repositories.Interfaces;
using backend.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace backend.Repositories;

public class UserRepository : MongoRepository<User>, IUserRepository
{
    public UserRepository(IOptions<MongoDbSettings> settings) : base(settings)
    {
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _collection.Find(u => u.Username == username).FirstOrDefaultAsync();
    }
}
