using Domain.Entities;
using Domain.RepositoryAbstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public abstract class Repository<T, TPrimaryKey> : IRepository<T, TPrimaryKey> where T
        : class, IEntity<TPrimaryKey>
    {
        protected readonly DbContext Context;
        private readonly DbSet<T> _entitySet;

        protected Repository(DbContext context)
        {
            Context = context;
            _entitySet = Context.Set<T>();
        }
        public virtual IQueryable<T> Get(Expression<Func<T, bool>>? predicate = null, bool asNoTracking = false) 
        {
            IQueryable<T> query = (predicate != null) ? _entitySet.Where(predicate) : _entitySet;
            return asNoTracking ? query.AsNoTracking() : query;
        }
        public virtual async Task<List<T>> GetAsync(CancellationToken cancellationToken, bool asNoTracking = false) 
        {
            return await Get().ToListAsync(cancellationToken);
        }

        public virtual T Add(T entity)
        {
            var objToReturn = _entitySet.Add(entity);
            return objToReturn.Entity;
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            return (await _entitySet.AddAsync(entity)).Entity;
        }

        public virtual void AddRange(List<T> entities)
        {
            var enumerable = entities as IList<T> ?? entities.ToList();
            _entitySet.AddRange(enumerable);
        }

        public virtual async Task AddRangeAsync(ICollection<T> entities)
        {
            if (entities == null || entities.Count == 0)
            {
                return;
            }
            await _entitySet.AddRangeAsync(entities);
        }

        public virtual void SaveChanges() 
        {
            Context.SaveChanges();
        }

        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default) 
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
    }
}
