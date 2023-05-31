namespace Acies.Docs.Models
{
    public interface ITemplateService
    {
        Task<IEnumerable<Template>> GetTemplatesByTagsAsync(IDictionary<string, string> tags);
        Task<Template> CreateTemplateAsync(TemplateCreateData createData);
        Task<Template> UpdateTemplateAsync(string key, TemplateUpdateData updateData);
        Task<TemplateVersion?> GetLatestVersionAsync(string key);
        Task<TemplateVersion?> GetTemplateVersionByKeyAndVersionAsync(string key, int version);
        Task<Template?> GetTemplateByKeyAsync(string key);
    }
}