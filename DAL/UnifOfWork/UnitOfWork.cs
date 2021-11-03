using DAL.Model;
using DAL.Repository;
using System;
using System.Collections.Generic;

namespace DAL.UnifOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WeSpreadCoreContext _context;
        private readonly Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        public UnitOfWork(WeSpreadCoreContext context)
        {
            _context = context;
        }

        public int Commit()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            var type = typeof(T);
            if (!repositories.ContainsKey(type))
            {
                var repositoryType = typeof(Repository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);
                repositories.Add(type, repositoryInstance);
            }
            return (IRepository<T>)repositories[type];
        }
    }
}
