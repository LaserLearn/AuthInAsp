using AuthInAsp.Contracts.Generic;
using Microsoft.EntityFrameworkCore;

namespace AuthInAsp.Repositories.Generic
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly Context_En _context;
        private readonly DbSet<T> _table;

        public GenericRepository(Context_En context)
        {
            _context = context;
            _table = _context.Set<T>();
        }

        public async Task Add(T entity)
        {
            await _table.AddAsync(entity);
        }

        public async Task Delete(List<int> ids)
        {
            var entities = new List<T>();

            foreach (var id in ids)
            {
                var entity = await Get(id);
                if (entity == null)
                    throw new Exception($"Entity with ID {id} not found.");

                entities.Add(entity);
            }

            _table.RemoveRange(entities);
        }

        public async Task<bool> Exists(int id)
        {
            return await _table.FindAsync(id) != null;
        }

        public async Task<T?> Get(int id)
        {
            return await _table.FindAsync(id);
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _table.ToListAsync();
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            _table.Update(entity);
        }
    }

}
