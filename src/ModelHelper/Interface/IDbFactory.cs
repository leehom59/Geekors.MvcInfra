using System;
using System.Data.Entity;

namespace ModelHelper.Interface
{
    public interface IDbFactory : IDisposable
    {
        /// <summary>
        /// </summary>
        bool Transaction { get; set; }

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

        //void ReleaseThreadDbFactory();
    }
}