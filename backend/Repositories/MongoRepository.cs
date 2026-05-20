
using System.Linq.Expressions;
using backend.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using backend.Repositories.Interfaces;

namespace backend.Repositories;

public class MongoRepository<TDocument> : IMongoRepository<TDocument> where TDocument : class
{
    protected readonly IMongoCollection<TDocument> _collection;

    public MongoRepository(IOptions<MongoDbSettings> settings)
    {
        var database = new MongoClient(settings.Value.ConnectionString)
                        .GetDatabase(settings.Value.DatabaseName);
        
        
        _collection = database.GetCollection<TDocument>(typeof(TDocument).Name);
    }

    public async Task<IEnumerable<TDocument>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<TDocument>> FilterByAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
        return await _collection.Find(filterExpression).ToListAsync();
    }

    public async Task<TDocument> GetByIdAsync(string id)
    {
        
        var filter = Builders<TDocument>.Filter.Eq("Id", id);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<TDocument> GetOneAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
        return await _collection.Find(filterExpression).FirstOrDefaultAsync();
    }

    public async Task InsertOneAsync(TDocument document)
    {
        await _collection.InsertOneAsync(document);
    }

    public async Task ReplaceOneAsync(string id, TDocument document)
    {
        var filter = Builders<TDocument>.Filter.Eq("Id", id);
        await _collection.ReplaceOneAsync(filter, document);
    }

    public async Task DeleteByIdAsync(string id)
    {
        var filter = Builders<TDocument>.Filter.Eq("Id", id);
        await _collection.DeleteOneAsync(filter);
    }
}
