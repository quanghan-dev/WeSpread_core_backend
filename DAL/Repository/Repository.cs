using DAL.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DAL.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly WeSpreadCoreContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(WeSpreadCoreContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet;
        }

        public T Get(int id)
        {
            return _dbSet.Find(id);
        }

        public T Get(string id)
        {
            return _dbSet.Find(id);
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
