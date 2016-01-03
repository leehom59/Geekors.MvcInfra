using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

namespace Geekors.MvcInfra.Interface
{
    public interface IRepository : IDisposable
    {
        IDbFactory DbFactory { get; }
        Type GetEntityType();
    }

    public interface IRepository<TEntity> : IRepository where TEntity : class
    {
        IBridge<TEntity> Bridge { get; set; }
        IQueryable<TEntity> GetEntities(Expression<Func<TEntity, bool>> where = null, params Expression<Func<TEntity, object>>[] includess);

        IQueryable<TEntity> GetPageEntities(string strBridgeWhere, string strSort, int pageSize, int pageIndex,
            out int total, params Expression<Func<TEntity, object>>[] includess);

        //IQueryable<TEntity> GetPageEntities(int pageSize, int pageIndex,
        //    out int total, params Expression<Func<TEntity, object>>[] includess);
        TEntity Get(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includess);
        void InsertOrUpdate(TEntity entity);
        void Insert(TEntity entity);
        void Delete(TEntity entityToDelete);
        void Delete(Expression<Func<TEntity, bool>> where);
        void Update(TEntity entityToUpdate);
        void Refresh(object model);
        void Refresh(IEnumerable models);
        void SaveChanges();
        Expression<Func<TEntity, bool>> GetEqualKeyFunc(object value);
        string GetModelKeyName();
        TEntity Get<TKey>(TKey key, params Expression<Func<TEntity, object>>[] includess);
        void Delete<TKey>(TKey key);
    }
}