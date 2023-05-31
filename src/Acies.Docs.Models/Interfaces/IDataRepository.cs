
namespace Acies.Docs.Models
{
    public interface IDataRepository<T>
    {
        Task<T?> GetAsync(string key, int version, ConcurrencyToken? concurrencyToken = null);
        Task<int> GetLatestVersionAsync(string key);
        Task<IEnumerable<string>> GetKeysByTagsAsync(IDictionary<string, string> tags);
        Task SaveAsync(T data, string key, int version, ConcurrencyToken? concurrencyToken = null);
    }
}