namespace Entities.Interfaces
{
    public interface ICacheManager
    {
        void SetCacheValue(string key, object value);
        object GetCacheValue(string key);
    }
}
