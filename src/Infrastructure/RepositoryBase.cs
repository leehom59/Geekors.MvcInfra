using System;
using System.Collections;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using Geekors.MvcInfra.Extensions;
using Geekors.MvcInfra.Interface;

namespace Geekors.MvcInfra.Infrastructure
{
    /// <summary>
    /// 非繼承型物件資料倉儲基底類別
    /// </summary>
    /// <typeparam name="TDbFactory">DbFactory</typeparam>
    /// <typeparam name="TEntity">物件類別</typeparam>
    public abstract class RepositoryBase<TDbFactory, TEntity> : IRepository<TEntity>
        where TDbFactory : IDbFactory, new()
        where TEntity : class
    {
        protected readonly IDbFactory _dbFactory;
        protected readonly ObjectContext ObjectContext;
        public string AddedFilter { private get; set; }

        public void ClearAddedFilter()
        {
            AddedFilter = null;
        }

        /// <summary>
        /// 建立資料倉儲類別（使用新資料連線）
        /// </summary>
        protected RepositoryBase() : this(new TDbFactory())
        {
        }

        /// <summary>
        /// 建立資料倉儲類別（使用新資料連線）
        /// </summary>
        /// <param name="transactionContainsInserts"></param>
        protected RepositoryBase(bool transactionContainsInserts)
            : this(new TDbFactory { TransactionContainsInserts = transactionContainsInserts })
        {
        }

        /// <summary>
        /// 建立資料倉儲類別（使用傳入之 dbFactory使用的資料連線）
        /// </summary>
        /// <param name="dbFactory"></param>
        protected RepositoryBase(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
            ObjectContext = ((IObjectContextAdapter) DbFactory.Get()).ObjectContext;
        }

        /// <summary>
        /// 資料庫橋接器
        /// </summary>
        public IBridge<TEntity> Bridge { get; set; }

        /// <summary>
        /// 取得物件型別
        /// </summary>
        /// <returns></returns>
        public Type GetEntityType()
        {
            return typeof (TEntity);
        }

        /// <summary>
        /// DbFactory
        /// </summary>
        public IDbFactory DbFactory
        {
            get { return _dbFactory; }
        }

        /// <summary>
        /// 取得所有物件實體
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includess">指定要包含在查詢結果中的相關物件。</param>
        /// <returns></returns>
        public IQueryable<TEntity> GetEntities(Expression<Func<TEntity, bool>> where = null,
            params Expression<Func<TEntity, object>>[] includess)
        {
            int total;
            if (Bridge != null)
            {
                string strWhere = "";
                if (where != null)
                {
                    strWhere = GetWhereClause(where);
                }
                return GetPageEntities(strWhere, "", 0, 0, out total, includess);
            }
            var entities = GetPageEntities("", "", 0, 0, out total, includess);
            if (where != null)
                entities = entities.Where(@where);
            return entities;
        }

        //public virtual IQueryable<TEntity> GetPageEntities(IEnumerable<IFilterDescriptor> filter,
        //    IList<SortDescriptor> sorts,
        //    int pageSize, int pageIndex, out int total, params Expression<Func<TEntity, object>>[] includess)
        //{
        //    IQueryable<TEntity> entities;
        //    IQueryable<TEntity> entitieCount = null;
        //    string strBridgeWhere = null;
        //    if (filter != null)
        //    {
        //        strBridgeWhere = FilterHelper.GetFilterDescriptor(filter);
        //        //if (string.IsNullOrEmpty(strBridgeWhere))
        //        //    strBridgeWhere = null;
        //    }
        //    var strSort = "";
        //    if (sorts != null && sorts.Count > 0)
        //    {
        //        sorts.ToList().ForEach(x =>
        //        {
        //            strSort += string.Format("[{0}] {1},", x.Member,
        //                x.SortDirection == ListSortDirection.Descending ? "DESC" : "");
        //        });
        //        strSort = strSort.TrimEnd(',');
        //    }
        //    else
        //    {
        //        strSort = string.Format("{0} ASC", GetModelKeyName());
        //    }

        //    if (Bridge == null)
        //    {
        //        if (filter != null && filter.Any())
        //        {
        //            entities = (IQueryable<TEntity>) GetObjectSet(includess).Where(filter);
        //            if (pageSize > 0 && pageIndex > 0)
        //                entitieCount = (IQueryable<TEntity>) GetObjectSet().Where(filter);
        //        }
        //        else
        //        {
        //            entities = GetObjectSet(includess);
        //            if (pageSize > 0 && pageIndex > 0)
        //                entitieCount = GetObjectSet();
        //        }
        //        if (!string.IsNullOrEmpty(strSort))
        //        {
        //            entities = entities.OrderBy(strSort);
        //        }
        //    }
        //    else
        //    {
        //        if (pageSize > 0 && pageIndex > 0)
        //        {
        //            entities = Bridge.Select(strBridgeWhere, strSort, pageSize, pageIndex);
        //        }
        //        else entities = Bridge.Select(strBridgeWhere);
        //    }
        //    if (pageSize > 0 && pageIndex > 0)
        //    {
        //        if (Bridge == null)
        //        {
        //            var param = Expression.Parameter(typeof (TEntity));
        //            var propertyName = GetModelKeyName();
        //            //important to use the Expression.Convert
        //            Expression conversion = Expression.Convert(Expression.Property(param, propertyName),
        //                Expression.Property(param, propertyName).Type);
        //            var type = Expression.Property(param, propertyName).Type;
        //            //var x = typeof (Expression).GetMethod("Lambda",
        //            //    new[] { typeof(Expression), typeof(ParameterExpression[]) });
        //            //MethodInfo method = x.MakeGenericMethod(type);
        //            //var lambda = method.Invoke(this, new object[] { conversion, param });
        //            //var lambda2 = Expression.Lambda<Func<TEntity, long>>(conversion, param);
        //            if (type.IsByRef)
        //                entities = entities.OrderBy(Expression.Lambda<Func<TEntity, dynamic>>(conversion, param));
        //            else
        //                switch (type.Name)
        //                {
        //                    case "Int64":
        //                        entities = entities.OrderBy(Expression.Lambda<Func<TEntity, long>>(conversion, param));
        //                        break;
        //                    case "UInt64":
        //                        entities = entities.OrderBy(Expression.Lambda<Func<TEntity, ulong>>(conversion, param));
        //                        break;
        //                    case "Int32":
        //                        entities = entities.OrderBy(Expression.Lambda<Func<TEntity, int>>(conversion, param));
        //                        break;
        //                    case "UInt32":
        //                        entities = entities.OrderBy(Expression.Lambda<Func<TEntity, uint>>(conversion, param));
        //                        break;
        //                    case "Double":
        //                        entities = entities.OrderBy(Expression.Lambda<Func<TEntity, double>>(conversion, param));
        //                        break;
        //                }
        //            entities = entities.Skip((pageIndex - 1)*pageSize).Take(pageSize);
        //            total = entitieCount.Count();
        //        }
        //        else
        //        {
        //            total = Bridge.SelectTotal(strBridgeWhere);
        //        }
        //    }
        //    else
        //        total = -1;
        //    return entities;
        //}

        /// <summary>
        /// 已分頁方式取得所有符合篩選條件物件實體
        /// </summary>
        /// <param name="strBridgeWhere"></param>
        /// <param name="strSort"></param>
        /// <param name="pageSize">分頁大小</param>
        /// <param name="pageIndex">目前分頁索引值</param>
        /// <param name="total">符合篩選條件物件實體總數</param>
        ///// <param name="expressions"></param>
        ///// <param name="includess">指定要包含在查詢結果中的相關物件。</param>
        ///// <param name="filter">篩選條件</param>
        ///// <param name="sorts">排序方式</param>
        /// <returns></returns>
        public IQueryable<TEntity> GetPageEntities(string strBridgeWhere, string strSort, int pageSize,
            int pageIndex, out int total, params Expression<Func<TEntity, object>>[] includess)
        {
            IQueryable<TEntity> entities;
            IQueryable<TEntity> entitieCount = null;

            if (Bridge == null)
            {
                entities = GetObjectSet(includess);
                entitieCount = GetObjectSet();
                if (!string.IsNullOrEmpty(strBridgeWhere))
                {
                    entities = entities.Where(strBridgeWhere);
                    if (pageSize > 0 && pageIndex > 0)
                        entitieCount = entitieCount.Where(strBridgeWhere);
                }
                if (!string.IsNullOrEmpty(strSort))
                {
                    entities = entities.OrderBy(strSort);
                }
                else if (!string.IsNullOrEmpty(strBridgeWhere))
                {
                    var propertyName = GetModelKeyName();
                    entities = entities.OrderBy(propertyName);
                }
            }
            else
            {
                if (pageSize > 0 && pageIndex > 0)
                {
                    entities = Bridge.Select(strBridgeWhere, strSort, pageSize, pageIndex);
                }
                else entities = Bridge.Select(strBridgeWhere);
            }
            if (pageSize > 0 && pageIndex > 0)
            {
                if (Bridge == null)
                {
                    var param = Expression.Parameter(typeof (TEntity));
                    var propertyName = GetModelKeyName();
                    //important to use the Expression.Convert
                    Expression conversion = Expression.Convert(Expression.Property(param, propertyName),
                        Expression.Property(param, propertyName).Type);
                    var type = Expression.Property(param, propertyName).Type;
                    //var x = typeof (Expression).GetMethod("Lambda",
                    //    new[] { typeof(Expression), typeof(ParameterExpression[]) });
                    //MethodInfo method = x.MakeGenericMethod(type);
                    //var lambda = method.Invoke(this, new object[] { conversion, param });
                    //var lambda2 = Expression.Lambda<Func<TEntity, long>>(conversion, param);
                    if (type.IsByRef)
                        entities = entities.OrderBy(Expression.Lambda<Func<TEntity, dynamic>>(conversion, param));
                    else
                        switch (type.Name)
                        {
                            case "Int64":
                                entities = entities.OrderBy(Expression.Lambda<Func<TEntity, long>>(conversion, param));
                                break;
                            case "UInt64":
                                entities = entities.OrderBy(Expression.Lambda<Func<TEntity, ulong>>(conversion, param));
                                break;
                            case "Int32":
                                entities = entities.OrderBy(Expression.Lambda<Func<TEntity, int>>(conversion, param));
                                break;
                            case "UInt32":
                                entities = entities.OrderBy(Expression.Lambda<Func<TEntity, uint>>(conversion, param));
                                break;
                            case "Double":
                                entities = entities.OrderBy(Expression.Lambda<Func<TEntity, double>>(conversion, param));
                                break;
                        }
                    entities = entities.Skip((pageIndex - 1)*pageSize).Take(pageSize);
                    total = entitieCount.Count();
                }
                else
                {
                    total = Bridge.SelectTotal(strBridgeWhere);
                }
            }
            else
                total = -1;
            return entities;
        }

        /// <summary>
        /// 取得單一物件實體
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includess">指定要包含在查詢結果中的相關物件。</param>
        /// <returns></returns>
        public TEntity Get(Expression<Func<TEntity, bool>> @where,
            params Expression<Func<TEntity, object>>[] includess)
        {
            if (DbFactory.TransactionContainsInserts)
            {
                var entity = GetNewEntity(@where.Compile());
                if (entity != null)
                    return entity;
            }
            return GetEntities(@where).FirstOrDefault();
        }

        /// <summary>
        /// 刪除物件
        /// </summary>
        /// <param name="where"></param>
        public virtual void Delete(Expression<Func<TEntity, bool>> @where)
        {
            TEntity model = null;
            foreach (var entity in GetEntities(@where))
            {
                model = entity;
                if (DbFactory.Get().Entry(model).State != EntityState.Deleted && BeforeDelete(model))
                {
                    DbFactory.Get().Entry(model).State = EntityState.Deleted;
                    AfterDelete(ref model);
                }
            }
            var sortable = model as ISortable;
            if (sortable != null)
                sortable.Sort(_dbFactory, model);
        }

        /// <summary>
        /// 新增或修改物件
        /// </summary>
        /// <param name="model">物件實體</param>
        public virtual void InsertOrUpdate(TEntity model)
        {
            var propertyName = GetModelKeyName();
            var propertyInfo = typeof (TEntity).GetProperties().First(p => p.Name == propertyName);
            if (propertyInfo.GetValue(model) == null ||
                propertyInfo.GetValue(model).Equals(propertyInfo.PropertyType.DefaultValue()))
                Insert(model);
            else
                Update(model);
        }

        /// <summary>
        ///     修改物件
        /// </summary>
        /// <param name="model">物件實體</param>
        public virtual void Update(TEntity model)
        {
            if (model != null && DbFactory.Get().Entry(model).State != EntityState.Added)
                if (BeforeUpdate(ref model))
                {
                    DbFactory.Get().Entry(model).State = EntityState.Modified;
                    //AfterUpdate(ref model);
                    var sortable = model as ISortable;
                    if (sortable != null)
                        sortable.Sort(_dbFactory, model);
                }
                else
                {
                    DbFactory.Get().Entry(model).State = EntityState.Unchanged;
                }
        }

        /// <summary>
        /// 新增物件
        /// </summary>
        /// <param name="model">物件實體</param>
        public virtual void Insert(TEntity model)
        {
            //if (model != null && DbFactory.Get().Entry(model).State != EntityState.Added && BeforeInsert(ref model))
            if (model != null && BeforeInsert(ref model))
            {
                var objectSet = ObjectContext.CreateObjectSet<TEntity>();
                objectSet.AddObject(model);
                AfterInsert(ref model);
                var sortable = model as ISortable;
                if (sortable != null)
                    sortable.Sort(_dbFactory, model);
                //if (set.EntitySet.ElementType.KeyProperties.First().IsStoreGeneratedIdentity)
                if (!_dbFactory.TransactionContainsInserts)
                    DbFactory.Get().SaveChanges();
            }
        }

        /// <summary>
        /// 刪除物件實體
        /// </summary>
        /// <param name="model">物件實體</param>
        public virtual void Delete(TEntity model)
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
                    DbFactory.Get().Entry(model).State = EntityState.Deleted;
                    var sortable = model as ISortable;
                    if (sortable != null)
                        sortable.Sort(_dbFactory, model);
                    AfterDelete(ref model);
                }
            }
        }

        /// <summary>
        /// 儲存所有變更儲存到基礎資料庫。(跨 Repository，並使用資料庫交易)
        /// </summary>
        public void SaveChanges()
        {
            try
            {
                DbFactory.BeginTransaction();
                DbFactory.Get().SaveChanges();
                DbFactory.Commit();
            }
            catch
            {
                DbFactory.Rollback();
                ObjectContext.Connection.Close();
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// 以特定實體的存放區資料重新整理快取資料（捨棄用戶端上的所有變更，並以存放區值重新整理值。）。
        /// </summary>
        /// <param name="model">物件實體</param>
        public void Refresh(object model)
        {
            ObjectContext.Refresh(RefreshMode.StoreWins, model);
        }

        /// <summary>
        /// 以特定實體的存放區資料重新整理快取資料（捨棄用戶端上的所有變更，並以存放區值重新整理值。）。
        /// </summary>
        /// <param name="models"></param>
        public void Refresh(IEnumerable models)
        {
            ObjectContext.Refresh(RefreshMode.StoreWins, models);
        }

        /// <summary>
        /// 釋放物件內容所使用的資源。
        /// </summary>
        public virtual void Dispose()
        {
            if (ObjectContext.Connection.State != ConnectionState.Closed)
            {
                DbFactory.Get().SaveChanges();
            }
            ObjectContext.Dispose();
            DbFactory.Dispose();
        }

        /// <summary>
        /// 取得實體類型之索引鍵屬性名稱（單一索引鍵屬性）
        /// </summary>
        /// <returns></returns>
        public virtual string GetModelKeyName()
        {
            var objectSet = ObjectContext.CreateObjectSet<TEntity>();
            return objectSet.EntitySet.ElementType.KeyProperties.First().Name;
        }

        /// <summary>
        /// 取得帶入值是否等於實體類型之索引鍵方法委派
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public Expression<Func<TEntity, bool>> GetEqualKeyFunc(object value)
        {
            var parameter = Expression.Parameter(typeof (TEntity));
            var propertyInfo = typeof (TEntity).GetProperty(GetModelKeyName());
            //x.ColumnName
            var left = Expression.Property(parameter, propertyInfo);
            //value (Constant Value)
            var right = propertyInfo.PropertyType == typeof (Guid) && value is string
                ? Expression.Constant(Guid.Parse((string) value))
                : Expression.Constant(value);
            //x.ColumnName == value
            var filter = Expression.Equal(left, right);
            //x => x.ColumnName == value
            return Expression.Lambda<Func<TEntity, bool>>(filter, parameter);
        }

        /// <summary>
        /// 取得所有物件實體
        /// </summary>
        /// <param name="includess">指定要包含在查詢結果中的相關物件。</param>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> GetObjectSet(params Expression<Func<TEntity, object>>[] includess)
        {
            var entities = ObjectContext.CreateObjectSet<TEntity>();
            foreach (var include in includess)
            {
                entities.Include(include);
            }
            return entities;
        }

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includess"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> @where,
            params Expression<Func<TEntity, object>>[] includess)
        {
            return GetEntities(@where, includess);
        }

        /// <summary>
        /// 新增前處理
        /// </summary>
        /// <param name="model">物件實體</param>
        /// <returns></returns>
        protected virtual bool BeforeInsert(ref TEntity model)
        {
            return true;
        }

        /// <summary>
        /// 新增後處理
        /// </summary>
        /// <param name="model">物件實體</param>
        protected virtual void AfterInsert(ref TEntity model)
        {
        }

        /// <summary>
        /// 修改前處理
        /// </summary>
        /// <param name="model">物件實體</param>
        /// <returns></returns>
        protected virtual bool BeforeUpdate(ref TEntity model)
        {
            return true;
        }

        ///// <summary>
        ///// 修改後處理
        ///// </summary>
        ///// <param name="model">物件實體</param>
        //protected virtual void AfterUpdate(ref TEntity model)
        //{
        //}

        /// <summary>
        /// 刪除前處理
        /// </summary>
        /// <param name="model">物件實體</param>
        /// <returns></returns>
        protected virtual bool BeforeDelete(TEntity model)
        {
            return true;
        }

        /// <summary>
        /// 刪除後處理
        /// </summary>
        /// <param name="model">物件實體</param>
        protected virtual void AfterDelete(ref TEntity model)
        {
        }

        //private Expression<Func<TEntity, bool>> GetEqualModelKeyFunc(TEntity model)
        //{
        //    var parameter = Expression.Parameter(typeof (TEntity));
        //    var propertyInfo = typeof (TEntity).GetProperty(GetModelKeyName());
        //    var left = Expression.Property(parameter, propertyInfo);
        //    var right = Expression.Constant(propertyInfo.GetValue(model));
        //    //var right = Expression.Constant(GetDefaultValue(propertyInfo.PropertyType));
        //    var filter = Expression.Equal(left, right);
        //    //x => x.ColumnName == value
        //    return Expression.Lambda<Func<TEntity, bool>>(filter, parameter);
        //}

        protected TEntity GetNewEntity(Func<TEntity, bool> @where)
        {
            var newEntites = ObjectContext.ObjectStateManager
                .GetObjectStateEntries(EntityState.Added).Select(o => o.Entity).OfType<TEntity>();
            return newEntites.FirstOrDefault(@where);
        }
        public static string GetWhereClause<T>(Expression<Func<T, bool>> expression)
        {
            return GetValueAsString(expression.Body);
        }

        public static string GetValueAsString(Expression expression)
        {
            var value = "";
            var equalty = "";
            var left = GetLeftNode(expression);
            var right = GetRightNode(expression);
            if (expression.NodeType == ExpressionType.Equal)
            {
                equalty = "=";
            }
            if (expression.NodeType == ExpressionType.AndAlso)
            {
                equalty = "AND";
            }
            if (expression.NodeType == ExpressionType.OrElse)
            {
                equalty = "OR";
            }
            if (expression.NodeType == ExpressionType.NotEqual)
            {
                equalty = "<>";
            }
            if (left is MemberExpression)
            {
                var leftMem = left as MemberExpression;
                value = string.Format("({0}{1}'{2}')", leftMem.Member.Name, equalty, "{0}");
            }
            if (right is ConstantExpression)
            {
                var rightConst = right as ConstantExpression;
                value = string.Format(value, rightConst.Value);
            }
            if (right is MemberExpression)
            {
                var rightMem = right as MemberExpression;
                var rightConst = rightMem.Expression as ConstantExpression;
                var member = rightMem.Member.DeclaringType;
                var type = rightMem.Member.MemberType;
                var val = member.GetField(rightMem.Member.Name).GetValue(rightConst.Value);
                value = string.Format(value, val);
            }
            if (value == "")
            {
                var leftVal = GetValueAsString(left);
                var rigthVal = GetValueAsString(right);
                value = string.Format("({0} {1} {2})", leftVal, equalty, rigthVal);
            }
            return value;
        }

        private static Expression GetLeftNode(Expression expression)
        {
            dynamic exp = expression;
            return ((Expression)exp.Left);
        }

        private static Expression GetRightNode(Expression expression)
        {
            dynamic exp = expression;
            return ((Expression)exp.Right);
        }
        public TEntity Get<TKey>(TKey key, params Expression<Func<TEntity, object>>[] includess)
        {
            if (Bridge == null)
                return GetEntities(null, includess).SingleOrDefault(GetEqualKeyFunc(key));
            var entity = Bridge.Get(key);
            return entity;
        }

        public void Delete<TKey>(TKey key)
        {
            var model = Get(key);
            Delete(model);
        }
    }
}