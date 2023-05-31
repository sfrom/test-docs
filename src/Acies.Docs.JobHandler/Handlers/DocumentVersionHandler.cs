using Acies.Docs.Models;
using Acies.Docs.Models.Interfaces;
using Common.Models;
using HandlebarsDotNet;
using HandlebarsDotNet.Extension.Json;
using Notifier;
using System.Text.Json;

namespace Acies.Docs.JobHandler.Handlers
{
    public class DocumentVersionHandler : BaseEventHandler<DocumentVersion>
    {
        private readonly IDocumentService _documentService;
        private readonly ITemplateService _templateService;
        private readonly ISNSMessageService _snsMessageService;
        private readonly TenantContext _tenantContext;
        private readonly ISerializer _serializer;

        public DocumentVersionHandler(INotificationService notificationService, IDocumentService documentService, ITemplateService templateService, ISNSMessageService snsMessageService, TenantContext tenantContext, ISerializer serializer) : base(notificationService)
        {
            _documentService = documentService;
            _templateService = templateService;
            _snsMessageService = snsMessageService;
            _tenantContext = tenantContext;
            _serializer = serializer;
        }

        public override string Resource => "DocumentVersion";

        protected override void OnUpdate(DocumentVersion data, IDictionary<string, string> attributes)
        {
            Console.WriteLine("DocumentVersion status event being handled: " + JsonSerializer.Serialize(data));
            _tenantContext.AccountId = attributes["AccountId"];
            var templateVersion = _templateService.GetTemplateVersionByKeyAndVersionAsync(data.Template, data.TemplateVersion).GetAwaiter().GetResult();
            _documentService.SetDocumentStatus(data, Status.Processing).GetAwaiter().GetResult();
            var document = _documentService.GetDocumentVersionByKeyAsync(data.Id, data.Version).GetAwaiter().GetResult();
            _snsMessageService.UpdateStatusAsync<DocumentVersion>(document, "DocumentVersion", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("Status", Status.Processing.ToString()) }).GetAwaiter().GetResult();
            if(templateVersion.Outputs != null)
            {
                var model = JsonDocument.Parse(document.Input.ToLower());
                var handleBars = Handlebars.Create();
                handleBars.Configuration.UseJson();
                foreach (var outp in templateVersion.Outputs)
                {
                    var template = handleBars.Compile(outp.Name.ToLower());
                    var replaced = template(model);
                    outp.Name = replaced + "." + outp.Type.ToString().ToLower();
                    var generatorInput = new OutputGeneratorInput()
                    {
                        Output = _serializer.Serialize(outp),
                        DocumentId = document.Id,
                        DocumentVersion = document.Version
                    };
                    _snsMessageService.UpdateStatusAsync(generatorInput, "OutputGenerator", new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("OutputType", outp.Type.ToString()), new KeyValuePair<string, string>("OutputName", outp.Name) }).GetAwaiter().GetResult();
                }
            }
        }
    }
}
