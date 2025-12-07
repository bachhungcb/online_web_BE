using System.Linq.Expressions;

namespace Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetById(Guid Id);
    IQueryable<T> GetAllAsQueryable();
    IEnumerable<T> Find(Expression<Func<T, bool>> expression);
    void Add(T entity);
    void AddRange(IEnumerable<T> entities);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    Task<int> CountAsync(Expression<Func<T, bool>> expression);
    void Update(T entity);
}