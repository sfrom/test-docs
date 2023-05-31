using Acies.Docs.Models;
using Acies.Docs.Services.Repositories;
using Newtonsoft.Json.Schema;

namespace Acies.Docs.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly ITemplateRepository _repository;
        
        public TemplateService(ITemplateRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Template>> GetTemplatesByTagsAsync(IDictionary<string, string> tags)
        {
            var keys = await _repository.GetKeysByTagsAsync(tags);
            var templatesData = await _repository.GetBatchAsync(keys);
            return templatesData;
        }

        public async Task<Template> CreateTemplateAsync(TemplateCreateData createData)
        {
            DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
            if (!string.IsNullOrWhiteSpace(createData?.Input?.Validation))
            {
                JSchema.Parse(createData.Input.Validation);
            }
            var template = new Template
            {
                Id = Guid.NewGuid().ToString(),
                Name = createData.Name,
                Managed = true,
                Version = 1,
                Tags = createData.Tags,
                CreatedAt = dto.ToUnixTimeSeconds(),
                UpdatedAt = dto.ToUnixTimeSeconds()
            };
            var templateVersion = new TemplateVersion
            {
                Id = template.Id,
                Name = createData.Name,
                CreatedAt = template.CreatedAt,
                Version = 1,
                Input = createData.Input,
                Outputs = createData.Outputs,
            };
            await _repository.SaveVersionAsync(templateVersion, templateVersion.Id, 1);
            await _repository.SaveAsync(template, template.Id, 1);
            return template;
        }

        public async Task<Template> UpdateTemplateAsync(string key, TemplateUpdateData updateData)
        {
            var concurrencyToken = new ConcurrencyToken();
            if (!string.IsNullOrWhiteSpace(updateData?.Input?.Validation))
            {
                JSchema.Parse(updateData.Input.Validation);
            }
            DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
            var template = await _repository.GetAsync(key);
            if (template == null) throw new ArgumentException($"Template with key {key} no found");
            template.Version++;
            template.UpdatedAt = dto.ToUnixTimeSeconds();

            var templateVersion = new TemplateVersion
            {
                Id = template.Id,
                Version = template.Version,
                Name = updateData.Name,
                CreatedAt = dto.ToUnixTimeSeconds(),
                Input = updateData.Input,
            };
            await _repository.SaveAsync(template, key, template.Version);
            await _repository.SaveVersionAsync(templateVersion, key, template.Version);
            return template;
        }

        public async Task<TemplateVersion?> GetTemplateVersionByKeyAndVersionAsync(string key, int version)
        {
            Console.WriteLine("Getting template version for key " + key + " and version " + version);
            var templateVersion = await _repository.GetVersionAsync(key, version);
            if (templateVersion == null)
            {
                throw new ArgumentException($"Template with key {key} not found");
            }
            return templateVersion;
        }

        public async Task<TemplateVersion?> GetLatestVersionAsync(string key)
        {
            var template = await _repository.GetLatestAsync(key);
            if (template == null) throw new ArgumentException($"Template with key {key} not found");
            return template;
        }

        public async Task<Template?> GetTemplateByKeyAsync(string key)
        {
            var template = await _repository.GetAsync(key);
            if (template == null) throw new ArgumentException($"Template with key {key} not found");
            return template;
        }
    }
}