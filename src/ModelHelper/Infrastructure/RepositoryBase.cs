using System;
using System.Collections;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using ModelHelper.Extensions;
using ModelHelper.Interface;

namespace ModelHelper.Infrastructure
{
    /// <summary>
    /// 非繼承型物件資料倉儲基底類別
    /// </summary>
    /// <typeparam name="TDbFactory">DbFactory</typeparam>
    /// <typeparam name="TEntity">物件類別</typeparam>
    public abstract class RepositoryBase<TDbFactory, TEntity> : IRepository<TEntity>
        where TDbFactory : IDbFactory, new()
        where TEntity : class, new()
    {
        protected ObjectContext ObjectContext => _objectContext ?? (_objectContext = ((IObjectContextAdapter) DbFactory.Get()).ObjectContext);

        private IDbFactory _dbFactory;
        private IDbFactory _threadDbFactory;
        private ObjectContext _objectContext;
        //public string AddedFilter { private get; set; }

        //public void ClearAddedFilter()
        //{
        //    AddedFilter = null;
        //}

        /// <summary>
        /// 建立資料倉儲類別（使用新資料連線）
        /// </summary>
        protected RepositoryBase() : this(new TDbFactory())
        {
        }

        /// <summary>
        /// 建立資料倉儲類別（使用新資料連線）
        /// </summary>
        /// <param name="transaction"></param>
        protected RepositoryBase(bool transaction)
            : this(new TDbFactory { Transaction = transaction })
        {
        }

        /// <summary>
        /// 建立資料倉儲類別（使用傳入之 dbFactory使用的資料連線）
        /// </summary>
        /// <param name="dbFactory"></param>
        protected RepositoryBase(IDbFactory dbFactory)
        {
            if (dbFactory != null)
            {
                DbFactory = dbFactory;
            }
        }

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
            get
            {
                return _dbFactory ?? ThreadDbFactory;
            }
            set
            {
                _dbFactory = value;
            }
        }

        public IDbFactory ThreadDbFactory
        {
            get { return _threadDbFactory; }
            set
            {
                _threadDbFactory = value;
                _objectContext = ((IObjectContextAdapter)_threadDbFactory.Get()).ObjectContext;
            }
        }

        /// <summary>
        /// 取得所有物件實體
        /// </summary>
        /// <param name="where"></param>
        /// <param name="includess">指定要包含在查詢結果中的相關物件。</param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> GetEntities(Expression<Func<TEntity, bool>> where = null,
            params Expression<Func<TEntity, object>>[] includess)
        {
            var entities = GetObjectSet(includess);
            if (where != null)
                entities = entities.Where(@where);
            return entities;
        }

        /// <summary>
        /// 已分頁方式取得所有符合篩選條件物件實體
        /// </summary>
        /// <param name="pageWhere"></param>
        /// <param name="pageSort"></param>
        /// <param name="pageSize">分頁大小</param>
        /// <param name="pageIndex">目前分頁索引值</param>
        /// <param name="total">符合篩選條件物件實體總數</param>
        /// <param name="includess"></param>
        /// <returns></returns>
        public virtual IQueryable<TEntity> GetPageEntities(string pageWhere, string pageSort, int pageSize,
            int pageIndex, out int total, params Expression<Func<TEntity, object>>[] includess)
        {
            var entities = GetObjectSet(includess);
            var entitieCount = GetObjectSet();
            if (!string.IsNullOrEmpty(pageWhere))
            {
                entities = entities.Where(pageWhere);
                if (pageSize > 0 && pageIndex > 0)
                    entitieCount = entitieCount.Where(pageWhere);
            }
            if (!string.IsNullOrEmpty(pageSort))
            {
                entities = entities.OrderBy(pageSort);
            }
            else if (!string.IsNullOrEmpty(pageWhere))
            {
                var propertyName = GetModelKeyName();
                entities = entities.OrderBy(propertyName);
            }
            if (pageSize > 0 && pageIndex > 0)
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
            if (DbFactory.Transaction)
            {
                var entity = GetNewEntity(@where.Compile());
                if (entity != null)
                    return entity;
            }
            return Find(@where).FirstOrDefault();
        }

        /// <summary>
        /// 刪除物件
        /// </summary>
        /// <param name="where"></param>
        public virtual void Delete(Expression<Func<TEntity, bool>> where)
        {
            TEntity model = null;
            foreach (var entity in Find(where))
            {
                model = entity;
                if (DbFactory.Get().Entry(model).State != EntityState.Deleted && BeforeDelete(model))
                {
                    DbFactory.Get().Entry(model).State = EntityState.Deleted;
                    AfterDelete(ref model);
                }
            }
            var sortable = model as ISortable;
            sortable?.Sort(DbFactory, model);
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
        /// 修改物件
        /// </summary>
        /// <param name="model">物件實體</param>
        public virtual void Update(TEntity model)
        {
            if (model != null && DbFactory.Get().Entry(model).State != EntityState.Added)
            {
                var oldModel = (TEntity)DbFactory.Get().Entry(model).OriginalValues.ToObject();
                if (BeforeUpdate(ref model, oldModel))
                {
                    DbFactory.Get().Entry(model).State = EntityState.Modified;
                    if (!DbFactory.Transaction)
                        DbFactory.Get().SaveChanges();
                    AfterUpdate(ref model);
                    var sortable = model as ISortable;
                    sortable?.Sort(DbFactory, model);
                }
                else
                {
                    DbFactory.Get().Entry(model).State = EntityState.Unchanged;
                }
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
                if (!DbFactory.Transaction)
                    DbFactory.Get().SaveChanges();
                AfterInsert(ref model);
                var sortable = model as ISortable;
                sortable?.Sort(DbFactory, model);
                //if (set.EntitySet.ElementType.KeyProperties.First().IsStoreGeneratedIdentity)
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
                    AfterDelete(ref model);
                }
                else if (DbFactory.Get().Entry(model).State != EntityState.Deleted && BeforeDelete(model))
                {
                    DbFactory.Get().Entry(model).State = EntityState.Deleted;
                    if (!DbFactory.Transaction)
                        DbFactory.Get().SaveChanges();
                    var sortable = model as ISortable;
                    sortable?.Sort(DbFactory, model);
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
            if (_objectContext != null && _objectContext.Connection.State != ConnectionState.Closed)
            {
                _objectContext.SaveChanges();
                _objectContext = null;
            }
            if (_dbFactory != null)
            {
                if (_dbFactory == _threadDbFactory)
                {
                    //_threadDbFactory.ReleaseThreadDbFactory();
                    _threadDbFactory = null;
                }
                _dbFactory.Dispose();
            }
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
        /// <param name="oldModel"></param>
        /// <returns></returns>
        protected virtual bool BeforeUpdate(ref TEntity model, TEntity oldModel)
        {
            return true;
        }

        /// <summary>
        /// 修改後處理
        /// </summary>
        /// <param name="model">物件實體</param>
        protected virtual void AfterUpdate(ref TEntity model)
        {
        }

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
                value = $"({leftMem.Member.Name}{equalty}'{"{0}"}')";
            }
            if (right is ConstantExpression)
            {
                var rightConst = right as ConstantExpression;
                value = string.Format(value, rightConst.Value);
            }
            var rightMem = right as MemberExpression;
            if (rightMem != null)
            {
                var rightConst = rightMem.Expression as ConstantExpression;
                var member = rightMem.Member.DeclaringType;
                //var type = rightMem.Member.MemberType;
                if (member != null && rightConst != null)
                {
                    var val = member.GetField(rightMem.Member.Name).GetValue(rightConst.Value);
                    value = string.Format(value, val);
                }
            }
            if (value == "")
            {
                var leftVal = GetValueAsString(left);
                var rigthVal = GetValueAsString(right);
                value = $"({leftVal} {equalty} {rigthVal})";
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

        public virtual TEntity GetByKey<TKey>(TKey key, params Expression<Func<TEntity, object>>[] includess)
        {
            return GetEntities(null, includess).SingleOrDefault(GetEqualKeyFunc(key));
        }

        public void Delete<TKey>(TKey key)
        {
            var model = GetByKey(key);
            Delete(model);
        }
    }
}