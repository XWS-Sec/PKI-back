using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Model.Shared
{
    public abstract class BaseRepository<TEntity, TKey> : IRepo<TEntity, TKey> where TEntity : class, new()
    {
        private bool _isDisposed;

        protected BaseRepository(AppDbContext context)
        {
            Context = context;
            Table = Context.Set<TEntity>();
        }

        protected AppDbContext Context { get; }
        protected DbSet<TEntity> Table { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        public int Add(TEntity entity, bool persist = true)
        {
            Table.Add(entity);
            return persist ? SaveChanges() : 0;
        }

        public int AddRange(IEnumerable<TEntity> entities, bool persist = true)
        {
            Table.AddRange(entities);
            return persist ? SaveChanges() : 0;
        }

        public int Update(TEntity entity, bool persist = true)
        {
            Table.Update(entity);
            return persist ? SaveChanges() : 0;
        }

        public int UpdateRange(IEnumerable<TEntity> entities, bool persist = true)
        {
            Table.UpdateRange(entities);
            return persist ? SaveChanges() : 0;
        }

        public int Delete(TEntity entity, bool persist = true)
        {
            Table.Remove(entity);
            return persist ? SaveChanges() : 0;
        }

        public int DeleteRange(IEnumerable<TEntity> entities, bool persist = true)
        {
            Table.RemoveRange(entities);
            return persist ? SaveChanges() : 0;
        }

        public TEntity GetById(TKey? id)
        {
            return Table.Find(id);
        }

        public DbSet<TEntity> GetAll()
        {
            return Table;
        }

        public void ExecuteQuery(string sql, object[] sqlParametersObjects)
        {
            Context.Database.ExecuteSqlRaw(sql, sqlParametersObjects);
        }

        public int SaveChanges()
        {
            return Context.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing) Context.Dispose();

            _isDisposed = true;
        }

        ~BaseRepository()
        {
            Dispose(false);
        }
    }
}