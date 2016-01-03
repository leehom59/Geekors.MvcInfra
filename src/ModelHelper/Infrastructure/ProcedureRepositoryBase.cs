using System;
using System.Linq;
using System.Linq.Expressions;
using ModelHelper.Interface;

namespace ModelHelper.Infrastructure
{
    public abstract class ProcedureRepositoryBase<TDbFactory, TEntity> : RepositoryBase<TDbFactory, TEntity>, IProcedureRepository<TEntity> where TDbFactory : IDbFactory, new()
        where TEntity : class, new()
    {
        /// <summary>
        /// 資料庫橋接器
        /// </summary>
        public IBridge<TEntity> Bridge { get; set; }
        protected ProcedureRepositoryBase()
            : this(new TDbFactory())
        {
        }

        protected ProcedureRepositoryBase(IDbFactory dbFactory)
            : base(dbFactory)
        {
        }

        public override IQueryable<TEntity> GetEntities(Expression<Func<TEntity, bool>> @where = null, params Expression<Func<TEntity, object>>[] includess)
        {
            int total;
            string strWhere = "";
            if (where != null)
            {
                strWhere = GetWhereClause(where);
                //var sqlString = ((ObjectQuery<TEntity>)ObjectContext.CreateObjectSet<TEntity>().Where(where)).ToTraceString();
                //var whereIndex = sqlString.IndexOf("WHERE ");
                //var xstrWhere = sqlString.Substring(whereIndex + 6);
            }
            return GetPageEntities(strWhere, "", 0, 0, out total, includess);
        }

        public override IQueryable<TEntity> GetPageEntities(string pageWhere, string pageSort, int pageSize, int pageIndex, out int total,
            params Expression<Func<TEntity, object>>[] includess)
        {
            var entities = pageSize > 0 && pageIndex > 0
                ? Bridge.Select(pageWhere, pageSort, pageSize, pageIndex)
                : Bridge.Select(pageWhere);
            total = pageSize > 0 && pageIndex > 0 ? Bridge.SelectTotal(pageWhere) : -1;
            return entities;
        }

        public int Total(string where = null)
        {
            return Bridge.SelectTotal(where);
        }
        /// <summary>
        /// 使用 SQL where 條件取得單一物件實體
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includess"></param>
        /// <returns></returns>
        public TEntity Get(string where, params Expression<Func<TEntity, object>>[] includess)
        {
            return Bridge.Select(where).FirstOrDefault();
        }
        /// <summary>
        /// 使用 SQL where 條件取得所有符合的物件實體
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public IQueryable<TEntity> Find(string where)
        {
            return Bridge.Select(where);
        }


        public override TEntity GetByKey<TKey>(TKey key, params Expression<Func<TEntity, object>>[] includess)
        {
            var entity = Bridge.Get(key);
            return entity;
        }

        protected override IQueryable<TEntity> GetObjectSet(params Expression<Func<TEntity, object>>[] includess)
        {
            return Bridge.Select(null);
        }
    }
}