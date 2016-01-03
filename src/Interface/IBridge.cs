using System.Linq;

namespace Geekors.MvcInfra.Interface
{
    public interface IBridge<out TEntity>
    {
        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        IQueryable<TEntity> Select(string where, string orderBy = null, int pageSize = 0, int page = 1);

        /// <summary>
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        int SelectTotal(string where);

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TEntity Get(object key);
    }
}