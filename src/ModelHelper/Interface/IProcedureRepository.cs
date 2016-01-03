namespace ModelHelper.Interface
{
    public interface IProcedureRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        IBridge<TEntity> Bridge { get; set; }
        int Total(string where);
    }
}
