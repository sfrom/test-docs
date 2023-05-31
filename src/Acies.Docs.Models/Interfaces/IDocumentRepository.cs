namespace Acies.Docs.Models
{
    public interface IDocumentRepository
    {
        Task<DocumentVersion?> GetLatestAsync(string key);
        Task SaveAsync(Document data, string key, int version);
        Task SaveVersionAsync(DocumentVersion data, string key, int version);
        Task<Document?> GetAsync(string key);
        Task<DocumentVersion?> GetVersionAsync(string key, int version);
        Task<IEnumerable<Document>?> GetBatchAsync(IEnumerable<string> keys);
        Task<int> GetLatestVersionAsync(string key);
        Task<IEnumerable<string>> GetKeysByTagsAsync(IDictionary<string, string> tags);
    }
}
