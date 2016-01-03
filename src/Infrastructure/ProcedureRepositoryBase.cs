using System;
using System.Linq;
using System.Linq.Expressions;
using Geekors.MvcInfra.Interface;

namespace Geekors.MvcInfra.Infrastructure
{
    public abstract class ProcedureRepositoryBase<TDbFactory, TEntity> : RepositoryBase<TDbFactory, TEntity>
        where TDbFactory : IDbFactory, new()
        where TEntity : class
    {
        protected ProcedureRepositoryBase()
            : this(new TDbFactory())
        {
        }

        protected ProcedureRepositoryBase(IDbFactory dbFactory)
            : base(dbFactory)
        {
        }

        protected override IQueryable<TEntity> GetObjectSet(Expression<Func<TEntity, object>>[] includess)
        {
            return Bridge.Select(null);
        }
    }
}