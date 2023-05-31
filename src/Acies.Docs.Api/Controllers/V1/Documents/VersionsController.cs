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
    public class VersionsController : CoreController
    {
        private readonly IDocumentService _documentService;
        private readonly TenantContext _tenantContext;

        public VersionsController(IDocumentService documentService, TenantContext tenantContext)
        {
            _documentService = documentService;
            _tenantContext = tenantContext;
        }

        [HttpGet("{id}")]
        [Authorize(Permission.DocumentGet)]
        public async Task<Response<DocumentVersionResource>> GetByIdAsync(string documentId, int id)
        {
            DocumentVersion? r;
            if (id != 0)
            {
                r = await _documentService.GetDocumentVersionByKeyAsync(documentId, id);
            }
            else
            {
                var doc = await _documentService.GetDocumentByKeyAsync(documentId);
                r = await _documentService.GetDocumentVersionByKeyAsync(doc.Id, doc.Version);
            }

            if (r is { })
            {
                var resource = r?.MapDocumentVersion(Request.Path);
                return new Response<DocumentVersionResource>(resource);
            }
            else
            {
                return ErrorResponse<DocumentVersionResource>.ResourceNotFound();
            }
        }
    }
}
