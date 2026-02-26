using Microsoft.EntityFrameworkCore;
using Voltix.Infrastructure.Context;
using Voltix.Infrastructure.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Voltix.Infrastructure.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            dbSet = _context.Set<T>();
        }
        public async Task<T> AddAsync(T entity, CancellationToken token)
        {
            var enityEntry = await dbSet.AddAsync(entity, token);
            return enityEntry.Entity;
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await dbSet.AddRangeAsync(entities);
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IQueryable<T>>? includes = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            CancellationToken ct = default,
            bool asNoTracking = true)
        {
            IQueryable<T> query = dbSet;

            if (asNoTracking)
                query = query.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            if (includes != null)
                query = includes(query);

            if (orderBy != null)
                query = orderBy(query);

            return await query.ToListAsync(ct);
        }

        public async Task<(IReadOnlyList<T> Items, int Total)> GetPagedAsync<TKey>(
            Expression<Func<T, bool>>? filter,
            Func<IQueryable<T>, IQueryable<T>>? includes,
            Expression<Func<T, TKey>> orderBy,
            bool ascending,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default,
            bool asNoTracking = true)
        {
            if (pageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            IQueryable<T> query = dbSet;

            if (asNoTracking)
                query = query.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            if (includes != null)
                query = includes(query);

            var total = await query.CountAsync(ct);

            query = ascending
                ? query.OrderBy(orderBy)
                : query.OrderByDescending(orderBy);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, total);
        }

        public IQueryable<T> Query(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IQueryable<T>>? includes = null,
            bool asNoTracking = true)
        {
            IQueryable<T> query = dbSet;

            if (asNoTracking)
                query = query.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            if (includes != null)
                query = includes(query);

            return query;
        }


        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            query = query.Where(filter);

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }

            return await query.FirstOrDefaultAsync();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            dbSet.Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            dbSet.UpdateRange(entities);
        }
    }
}
