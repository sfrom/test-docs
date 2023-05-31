using Acies.Docs.Api.Models;
using Acies.Docs.Models;
using Acies.Docs.Services.Amazon;
using Common.Attributes;
using Common.Controllers;
using Logger.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Acies.Docs.Api.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("documents/{documentId}/versions/{version}/output/{outputType}/accesses")]
    [ApiController]
    public class RendererController : CoreController
    {
        private readonly IHtmlRenderer _htmlRenderer;
        private readonly IDocumentService _documentService;
        private readonly ITemplateService _templateService;
        private readonly ILogService _logger;
        private readonly IOptionsSnapshot<DynamoDbDataRepositoryOptions> _options;

        public RendererController(IHtmlRenderer htmlRenderer, IDocumentService documentService, ITemplateService templateService, ILogService logger, IOptionsSnapshot<DynamoDbDataRepositoryOptions> options)
        {
            _options = options;
            _htmlRenderer = htmlRenderer;
            _documentService = documentService;
            _templateService = templateService;
            _logger = logger;
        }

        [HttpGet()]
        [Authorize(Permission.RedererGet)]
        public async Task<ActionResult<string>> GetAsync(Guid documentId, int version, OutputTypes outputType)
        {
            try
            {
                var documentVersion = await _documentService.GetLatestVersionAsync(documentId.ToString());
                if (documentVersion == null) throw new ArgumentException($"Unknown document {documentId}", "documentId");
                var templateV = await _templateService.GetTemplateVersionByKeyAndVersionAsync(documentVersion.Template, documentVersion.TemplateVersion);
                if (templateV == null) throw new ArgumentException($"Unknown template id {documentVersion.Template} version {documentVersion.TemplateVersion}", "templateVersion");
                var template = await _templateService.GetTemplateByKeyAsync(documentVersion.Template);
                if (template == null) throw new ArgumentException($"Unknown template id {documentVersion.Template}", "templateId");

                var generatorInput = new GeneratorInput
                {
                    Template = template,
                    TemplateVersion = templateV,
                    DocumentVersion = documentVersion,
                };

                if (outputType == OutputTypes.Pdf)
                {
                    var html = await _htmlRenderer.GenerateAsync<PdfOutput>(generatorInput);
                    return Ok(html);
                }
                else
                {
                    return BadRequest("Unsupported format.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex.Message, "RenderedController.GetAsync", ex, documentId);
                return BadRequest(ex.Message);
            }
        }

        //[HttpGet()]
        //public async Task<ActionResult<string>> GetAsync(Guid account, Guid templateId, int templateVersion, Guid documentId, int version, OutputTypes outputType)
        //{
        //    try
        //    {
        //        var template = await _templateService.GetTemplateByKeyAsync(templateId.ToString());
        //        if (template == null) throw new ArgumentException($"Unknown template {templateId}", "templateId");
        //        var templateV = await _templateService.GetTemplateVersionByKeyAndVersionAsync(templateId.ToString(), templateVersion);
        //        if (templateV == null) throw new ArgumentException($"Unknown template version {templateVersion}", "templateVersion");
        //        var documentVersion = await _documentService.GetLatestVersionAsync(documentId.ToString());
        //        if (documentVersion == null) throw new ArgumentException($"Unknown document {documentId}", "documentId");

        //        var generatorInput = new GeneratorInput
        //        {
        //            Template = template,
        //            TemplateVersion = templateV,
        //            DocumentVersion = documentVersion,
        //        };

        //        if (outputType == OutputTypes.Pdf)
        //        {
        //            var html = await _htmlRenderer.GenerateAsync<PdfOutput>(generatorInput);
        //            return Ok(html);
        //        }
        //        else
        //        {
        //            return BadRequest("Unsupported format.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //        return BadRequest(ex.Message);
        //    }
        //}
    }
}