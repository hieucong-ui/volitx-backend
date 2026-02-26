using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;

namespace Voltix.Infrastructure.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IQueryable<T>>? includes = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            CancellationToken ct = default,
            bool asNoTracking = true);
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);
        Task<T> AddAsync(T entity, CancellationToken token);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        Task<(IReadOnlyList<T> Items, int Total)> GetPagedAsync<TKey>(
            Expression<Func<T, bool>>? filter,
            Func<IQueryable<T>, IQueryable<T>>? includes,
            Expression<Func<T, TKey>> orderBy,
            bool ascending,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default,
            bool asNoTracking = true);
        IQueryable<T> Query(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IQueryable<T>>? includes = null,
            bool asNoTracking = true);
    }
}
