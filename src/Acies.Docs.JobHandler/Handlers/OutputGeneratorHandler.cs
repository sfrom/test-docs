using Acies.Docs.Models;
using Acies.Docs.Models.Exceptions;
using Acies.Docs.Models.Interfaces;
using Common.Models;
using Notifier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Acies.Docs.JobHandler.Handlers
{
    public class OutputGeneratorHandler : BaseEventHandler<OutputGeneratorInput>
    {
        private readonly TenantContext _tenantContext;
        private readonly ISerializer _serializer;
        private readonly IEnumerable<IOutputGenerator> _generators;
        private readonly ITemplateService _templateService;
        private readonly IDocumentService _documentService;

        public OutputGeneratorHandler(ITemplateService templateService, IDocumentService documentService, INotificationService notificationService, TenantContext tenantContext, ISerializer serializer, IEnumerable<IOutputGenerator> generators) : base(notificationService)
        {
            _templateService = templateService;
            _documentService = documentService;
            _tenantContext = tenantContext;
            _serializer = serializer;
            _generators = generators;
        }
        public override string Resource => "OutputGenerator";

        protected override void OnUpdate(OutputGeneratorInput data, IDictionary<string, string> attributes)
        {
            try
            {
                Console.WriteLine("Output generator handler executing");
                _tenantContext.AccountId = attributes["AccountId"];
                DocumentVersion = _documentService.GetDocumentVersionByKeyAsync(data.DocumentId, data.DocumentVersion).GetAwaiter().GetResult();
                var templateVersion = _templateService.GetTemplateVersionByKeyAndVersionAsync(DocumentVersion.Template, DocumentVersion.TemplateVersion).GetAwaiter().GetResult();
                var outputType = OutputTypes.None;
                Enum.TryParse(attributes["OutputType"], out outputType);
                var generatorInput = new GeneratorInput()
                {
                    TemplateVersion = templateVersion,
                    DocumentVersion = DocumentVersion,
                };
                var outputGenerator = _generators.FirstOrDefault(c => c.OutputType == outputType);
                if (outputGenerator != null)
                {
                    outputGenerator.GenerateAsync(generatorInput, data.Output).GetAwaiter().GetResult();
                }
                else
                {
                    throw new UnsupportedOutputTypeException("Output type not supported or generator not registered");
                }
            }
            catch(Exception ex)
            {
                _documentService.SetOutputStatus(DocumentVersion, attributes["OutputName"], Status.Failed);
                throw ex;
            }
        }

        public DocumentVersion DocumentVersion { get; set; }
    }
}