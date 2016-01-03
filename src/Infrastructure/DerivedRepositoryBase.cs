using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Geekors.MvcInfra.Interface;

namespace Geekors.MvcInfra.Infrastructure
{
    /// <summary>
    ///     繼承型物件資料倉儲基底類別
    /// </summary>
    /// <typeparam name="TDbFactory">DbFactory</typeparam>
    /// <typeparam name="TBaseEntity">基底物件類別</typeparam>
    /// <typeparam name="TEntity">物件類別</typeparam>
    public abstract class DerivedRepositoryBase<TDbFactory, TBaseEntity, TEntity> : RepositoryBase<TDbFactory, TEntity>
        where TDbFactory : IDbFactory, new()
        where TBaseEntity : class
        where TEntity : class, TBaseEntity
    {
        /// <summary>
        /// 建立資料倉儲類別（使用新資料連線）
        /// </summary>
        protected DerivedRepositoryBase()
            : this(new TDbFactory())
        {
        }
        /// <summary>
        /// 建立資料倉儲類別（使用新資料連線）
        /// </summary>
        /// <param name="transactionContainsInserts"></param>
        protected DerivedRepositoryBase(bool transactionContainsInserts = false)
            : this(new TDbFactory { TransactionContainsInserts = transactionContainsInserts })
        {
        }

        /// <summary>
        ///     建立資料倉儲類別（使用傳入之 dbFactory使用的資料連線）
        /// </summary>
        /// <param name="dbFactory"></param>
        protected DerivedRepositoryBase(IDbFactory dbFactory)
            : base(dbFactory)
        {
        }

        /// <summary>
        ///     取得所有物件實體
        /// </summary>
        /// <param name="includess">指定要包含在查詢結果中的相關物件。</param>
        /// <returns></returns>
        protected override IQueryable<TEntity> GetObjectSet(Expression<Func<TEntity, object>>[] includess)
        {
            var entities = ObjectContext.CreateObjectSet<TBaseEntity>().OfType<TEntity>();
            foreach (var include in includess)
            {
                entities.Include(include);
            }
            return entities;
        }

        /// <summary>
        ///     取得實體類型之索引鍵屬性名稱（單一索引鍵屬性）
        /// </summary>
        /// <returns></returns>
        public override string GetModelKeyName()
        {
            var set = ObjectContext.CreateObjectSet<TBaseEntity>();
            return set.EntitySet.ElementType.KeyProperties.First().Name;
        }

        /// <summary>
        ///     新增物件
        /// </summary>
        /// <param name="model">物件實體</param>
        public override void Insert(TEntity model)
        {
            if (model != null && DbFactory.Get().Entry(model).State != EntityState.Added && BeforeInsert(ref model))
            {
                var set = ObjectContext.CreateObjectSet<TBaseEntity>();
                set.AddObject(model);
                AfterInsert(ref model);
                var sortable = model as ISortable;
                if (sortable != null)
                    sortable.Sort(_dbFactory, model);
                if (!_dbFactory.TransactionContainsInserts)
                    DbFactory.Get().SaveChanges();
            }
        }

        /// <summary>
        ///     刪除物件
        /// </summary>
        /// <param name="where"></param>
        public override void Delete(TEntity model)
        {
            if (model != null)
            {
                if (DbFactory.Get().Entry(model).State == EntityState.Added)
                {
                    DbFactory.Get().Entry(model).State = EntityState.Detached;
                    return;
                }
                if (DbFactory.Get().Entry(model).State != EntityState.Deleted && BeforeDelete(model))
                {
                    ObjectContext.CreateObjectSet<TBaseEntity>().DeleteObject(model);
                    var sortable = model as ISortable;
                    if (sortable != null)
                        sortable.Sort(_dbFactory, model);
                    AfterDelete(ref model);
                }
            }
        }
    }
}