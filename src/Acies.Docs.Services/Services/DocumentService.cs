using Acies.Docs.Models;
using Acies.Docs.Models.Interfaces;
using Acies.Docs.Services.Repositories;
using Common.Models;
using HandlebarsDotNet;
using HandlebarsDotNet.Extension.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Text.Json;

namespace Acies.Docs.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IDocumentRepository _repository;
        private readonly ITemplateRepository _templateRepository;
        private readonly ISNSMessageService _snsMessageService;
        private readonly TenantContext _tenantContext;
        private readonly IValidationService _validationService;

        public DocumentService(IDocumentRepository repository, ITemplateRepository templateRepository, ISNSMessageService snsMessageService, TenantContext tenantContext, IValidationService validationService)
        {
            _repository = repository;
            _templateRepository = templateRepository;
            _snsMessageService = snsMessageService;
            _tenantContext = tenantContext;
            _validationService = validationService;
        }

        public async Task<IEnumerable<Document>> GetDocumentsByTagsAsync(IDictionary<string, string> tags)
        {
            var keys = await _repository.GetKeysByTagsAsync(tags);
            var documentsData = await _repository.GetBatchAsync(keys);
            return documentsData;
        }

        public async Task<DocumentResponse> CreateDocumentAsync(DocumentCreateData createData)
        {
            var response = new DocumentResponse();

            DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
            var document = new Document
            {
                Id = Guid.NewGuid().ToString(),
                Version = 1,
                Status = Status.Pending,
                Template = createData.TemplateId,
                TemplateVersion = createData.TemplateVersion,
                CreatedAt = dto.ToUnixTimeSeconds(),
                UpdatedAt = dto.ToUnixTimeSeconds(),
                Tags = createData.Tags,
            };

            var templateVersion = await _templateRepository.GetVersionAsync(createData.TemplateId, createData.TemplateVersion);
            if (templateVersion == null)
            {
                response.Success = false;
                response.ErrorMessage = $"Template with key {createData.TemplateId} and version {createData.TemplateVersion} not found";
            }
            var validationResponse = new ValidationResponse();
            if (templateVersion.Input != null)
            {
                validationResponse = _validationService.ValidateJson(createData.Input, templateVersion.Input.Validation);
            }

            if (validationResponse.Success)
            {
                var documentVersion = new DocumentVersion
                {
                    Id = document.Id,
                    Status = Status.Pending,
                    Template = createData.TemplateId,
                    TemplateVersion = createData.TemplateVersion,
                    CreatedAt = document.CreatedAt,
                    Input = createData.Input,
                    Version = 1,
                    Output = GenerateDocumentOutputs(createData.Input, templateVersion),
                };

                await _repository.SaveVersionAsync(documentVersion, documentVersion.Id, 1);
                await _repository.SaveAsync(document, document.Id, 1);
                await PublishDocumentStatusUpdatedEvent(documentVersion, Status.Pending.ToString());
                response.Document = document;
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = validationResponse.ErrorMessage;
            }
            return response;
        }

        public async Task<DocumentResponse> UpdateDocumentAsync(string key, DocumentUpdateData updateData)
        {
            var response = new DocumentResponse();
            DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
            var document = await _repository.GetAsync(key);
            if (document == null) throw new ArgumentException($"Document with key {key} not found");
            document.Version++;
            document.UpdatedAt = dto.ToUnixTimeSeconds();

            var templateVersion = await _templateRepository.GetVersionAsync(updateData.TemplateId, updateData.TemplateVersion);
            if (templateVersion == null)
            {
                response.Success = false;
                response.ErrorMessage = $"Template with key {updateData.TemplateId} and version {updateData.TemplateVersion} not found";
            }

            var validationResponse = new ValidationResponse(); // _validationService.ValidateInput(updateData.Input, templateVersion?.Input);

            if (validationResponse.Success)
            {

                var documentVersion = new DocumentVersion
                {
                    Id = key,
                    Version = document.Version,
                    Template = updateData.TemplateId,
                    TemplateVersion = updateData.TemplateVersion,
                    Status = Status.Pending,
                    CreatedAt = dto.ToUnixTimeSeconds(),
                    Input = updateData.Input,
                    Output = GenerateDocumentOutputs(updateData.Input, templateVersion)
                };
                await _repository.SaveAsync(document, key, document.Version);
                await _repository.SaveVersionAsync(documentVersion, key, document.Version);
                response.Document = document;
            }
            else
            {
                response.Success = false;
                response.ErrorMessage = validationResponse.ErrorMessage;
            }
            return response;
        }

        public async Task SetDocumentStatus(DocumentVersion documentVersion, Status status)
        {
            var document = await _repository.GetVersionAsync(documentVersion.Id, documentVersion.Version);
            if (document is null) return;

            document.Status = status;
            await _repository.SaveVersionAsync(document, document.Id, document.Version);
        }

        public async Task SetOutputStatus(DocumentVersion documentVersion, string outputName, Status status, string? externalPath = null)
        {
            var document = await _repository.GetVersionAsync(documentVersion.Id, documentVersion.Version);
            if (document is null) return;

            var output = document.Output!.FirstOrDefault(c => c.Name.ToLower() == outputName.ToLower());

            if (output is null) return;

            output.Status = status;

            if (externalPath is { })
            {
                output.ExternalPath = externalPath;
            }

            await _repository.SaveVersionAsync(document, document.Id, document.Version);
        }

        public async Task<DocumentVersion?> GetLatestVersionAsync(string key)
        {
            var document = await _repository.GetLatestAsync(key);
            if (document == null) throw new ArgumentException($"Document with key {key} not found");
            return document;
        }

        public async Task<DocumentVersion?> GetDocumentVersionByKeyAsync(string key, int version)
        {
            var item = await _repository.GetVersionAsync(key, version);
            if (item != null)
            {
                return item;
            }
            return new DocumentVersion();
        }

        public async Task<Document?> GetDocumentByKeyAsync(string key)
        {
            var item = await _repository.GetAsync(key);
            if (item != null)
            {
                return item;
            }
            return new Document();
        }

        public List<DocumentOutput> GenerateDocumentOutputs(string input, TemplateVersion templateVersion)
        {
            var documentOutputs = new List<DocumentOutput>();
            if (templateVersion?.Outputs?.Count() > 0)
            {
                var model = JsonDocument.Parse(input.ToLower());

                var handleBars = Handlebars.Create();
                handleBars.Configuration.UseJson();

                foreach (var output in templateVersion.Outputs)
                {
                    var template = handleBars.Compile(output.Name.ToLower());
                    var replaced = template(model);
                    documentOutputs.Add(new DocumentOutput()
                    {
                        Status = Status.Pending,
                        Type = output.Type,
                        Name = replaced + "." + output.Type.ToString().ToLower(),
                        Id = Guid.NewGuid().ToString()
                    });
                }
            }
            Console.WriteLine(JsonConvert.SerializeObject(documentOutputs));
            return documentOutputs;
        }

        private async Task PublishDocumentStatusUpdatedEvent(DocumentVersion document, string status)
        {
            var attributes = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Status", status) };

            await _snsMessageService.UpdateStatusAsync(document, "DocumentVersion", attributes);
        }
    }
}