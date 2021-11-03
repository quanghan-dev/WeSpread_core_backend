using DAL.Repository;
using System;

namespace DAL.UnifOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> GetRepository<T>() where T : class;

        int Commit();
    }
}
