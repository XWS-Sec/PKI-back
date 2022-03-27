using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Model.Shared
{
    public interface IRepo<TEntity, TKey> : IDisposable where TEntity : class
    {
        int Add(TEntity entity, bool persist = true);
        int AddRange(IEnumerable<TEntity> entities, bool persist = true);
        int Update(TEntity entity, bool persist = true);
        int UpdateRange(IEnumerable<TEntity> entities, bool persist = true);
        int Delete(TEntity entity, bool persist = true);
        int DeleteRange(IEnumerable<TEntity> entities, bool persist = true);

        TEntity GetById(TKey? id);

        DbSet<TEntity> GetAll();

        void ExecuteQuery(string sql, object[] sqlParametersObjects);

        int SaveChanges();
    }
}