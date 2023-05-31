using Acies.Docs.Models;

namespace Acies.Docs.Services.Repositories
{
    public interface ITemplateRepository
    {
        Task<TemplateVersion?> GetLatestAsync(string key);
        Task SaveAsync(Template data, string key, int version);
        Task SaveVersionAsync(TemplateVersion data, string key, int version);
        Task<Template?> GetAsync(string key);
        Task<TemplateVersion?> GetVersionAsync(string key, int version);
        Task<IEnumerable<Template>?> GetBatchAsync(IEnumerable<string> keys);
        Task<IEnumerable<string>> GetKeysByTagsAsync(IDictionary<string, string> tags);
    }
}
