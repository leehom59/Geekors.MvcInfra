using System;
using System.Data.Entity;

namespace Geekors.MvcInfra.Interface
{
    public interface IDbFactory : IDisposable
    {
        /// <summary>
        /// </summary>
        bool TransactionContainsInserts { get; set; }

        /// <summary>
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// </summary>
        void Rollback();

        /// <summary>
        /// </summary>
        void Commit();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        DbContext Get();
    }
}