namespace Geekors.MvcInfra.Interface
{
    public interface ISortable
    {
        void Sort(IDbFactory dbFactory, object entity);
    }
}