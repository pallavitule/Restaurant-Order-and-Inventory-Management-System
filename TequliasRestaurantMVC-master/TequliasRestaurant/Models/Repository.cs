using Microsoft.EntityFrameworkCore;
using TequliasRestaurant.Data;
using System.Linq.Expressions;

namespace TequliasRestaurant.Models
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected ApplicationDbContext Context { get; set; }
        private DbSet<T> DbSet { get; set; }

        public Repository(ApplicationDbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await DbSet.AddAsync(entity);
            await Context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            T entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                DbSet.Remove(entity);
                await Context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id, QueryOptions<T> options)
        {
            IQueryable<T> query = DbSet;

            if (options.HasWhere)
                query = query.Where(options.Where);

            if (options.HasOrderBy)
                query = query.OrderBy(options.OrderBy);

            foreach (string include in options.GetIncludes())
                query = query.Include(include);

            var key = Context.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.FirstOrDefault();
            if (key == null)
                throw new InvalidOperationException("Primary key not found.");

            string primaryKeyName = key.Name;
            return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, primaryKeyName) == id);
        }

        public async Task UpdateAsync(T entity)
        {
            Context.Update(entity);
            await Context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllByIdAsync<TKey>(TKey id, string propertyName, QueryOptions<T> options)
        {
            IQueryable<T> query = DbSet;

            if (options.HasWhere)
                query = query.Where(options.Where);

            if (options.HasOrderBy)
                query = query.OrderBy(options.OrderBy);

            foreach (string include in options.GetIncludes())
                query = query.Include(include);

            query = query.Where(e => EF.Property<TKey>(e, propertyName).Equals(id));

            return await query.ToListAsync();
        }
    }
}
