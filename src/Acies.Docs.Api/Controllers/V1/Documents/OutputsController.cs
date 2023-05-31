using Acies.Docs.Models;
using Microsoft.AspNetCore.Mvc;
using Acies.Docs.Api.Models;
using Common.Controllers;
using Common.Attributes;
using Common.Models;

namespace Acies.Docs.Api.Controllers.V1.Documents
{
    [ApiVersion("1.0")]
    [Route(nameof(Documents) + "/{documentId}/[controller]")]
    [ApiController]
    public class OutputsController : CoreController
    {
        private readonly IDocumentService _documentService;
        private readonly TenantContext _tenantContext;
        private readonly IOutputService _outputService;

        public OutputsController(IDocumentService documentService, TenantContext tenantContext, IOutputService outputService)
        {
            _documentService = documentService;
            _tenantContext = tenantContext;
            _outputService = outputService;
        }

        [HttpGet]
        [Authorize(Permission.DocumentGet)]
        public async Task<Response<IEnumerable<OutputResource>>> GetByIdAsync(string documentId)
        {
            var tags = new Dictionary<string, string>();
            foreach (var prop in Request.Query)
            {
                tags.Add(prop.Key, prop.Value);
            }

            int ver;
            DocumentVersion? r;
            if (tags.ContainsKey("version") && int.TryParse(tags["version"], out ver) && ver > 0)
            {
                r = await _documentService.GetDocumentVersionByKeyAsync(documentId, ver);

            }
            else
            {
                var doc = await _documentService.GetDocumentByKeyAsync(documentId);
                r = await _documentService.GetDocumentVersionByKeyAsync(doc.Id, doc.Version);
            }

            if (r is { })
            {
                if (r.Output == null || r.Output.Count() == 0) return ErrorResponse<IEnumerable<OutputResource>>.ResourceNotFound();

                var resources = new List<OutputResource>();
                foreach (var output in r.Output)
                {
                    var resource = new OutputResource()
                    {
                        Name = output.Name,
                        Status = output.Status.ToString(),
                        Type = output.Type.ToString(),
                        Self = "/documents/" + documentId + "/outputs/" + output.Id
                    };
                    resources.Add(resource);
                }

                return new Response<IEnumerable<OutputResource>>(resources);
            }
            else
            {
                return ErrorResponse<IEnumerable<OutputResource>>.ResourceNotFound();
            }
        }

        [HttpGet("{outputId}")]
        [Authorize(Permission.DocumentGet)]
        public async Task<Response<OutputResource>> GetByIdAsync(string documentId, string outputId)
        {
            var tags = new Dictionary<string, string>();
            foreach (var prop in Request.Query)
            {
                tags.Add(prop.Key, prop.Value);
            }

            int ver;
            DocumentVersion? r;
            if (tags.ContainsKey("version") && int.TryParse(tags["version"], out ver) && ver > 0)
            {
                r = await _documentService.GetDocumentVersionByKeyAsync(documentId, ver);

            }
            else
            {
                var doc = await _documentService.GetDocumentByKeyAsync(documentId);
                r = await _documentService.GetDocumentVersionByKeyAsync(doc.Id, doc.Version);
            }

            if (r is { })
            {
                var output = r.Output!.FirstOrDefault(c => c.Id.ToLower() == outputId.ToLower());

                if (output is null) return ErrorResponse<OutputResource>.ResourceNotFound();

                var resource = new OutputResource()
                {
                    Name = output.Name,
                    Status = output.Status.ToString(),
                    Type = output.Type.ToString(),
                    Self = "/documents/" + documentId + "/outputs/" + output.Id
                };

                return new Response<OutputResource>(resource);
            }
            else
            {
                return ErrorResponse<OutputResource>.ResourceNotFound();
            }
        }
    }
}
