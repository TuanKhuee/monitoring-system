// Repositories/Interfaces/IMongoRepository.cs
using System.Linq.Expressions;

namespace backend.Repositories.Interfaces;

public interface IMongoRepository<TDocument> where TDocument : class
{
    // Lấy toàn bộ dữ liệu
    Task<IEnumerable<TDocument>> GetAllAsync();
    
    // Lấy theo điều kiện (ví dụ: lấy project theo UserId)
    Task<IEnumerable<TDocument>> FilterByAsync(Expression<Func<TDocument, bool>> filterExpression);
    
    // Lấy 1 document theo Id
    Task<TDocument> GetByIdAsync(string id);
    
    // Lấy 1 document theo điều kiện
    Task<TDocument> GetOneAsync(Expression<Func<TDocument, bool>> filterExpression);
    
    // Thêm mới
    Task InsertOneAsync(TDocument document);
    
    // Cập nhật (thay thế toàn bộ)
    Task ReplaceOneAsync(string id, TDocument document);
    
    // Xóa
    Task DeleteByIdAsync(string id);
}
