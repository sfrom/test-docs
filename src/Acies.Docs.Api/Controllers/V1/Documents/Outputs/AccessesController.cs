using Acies.Docs.Models;
using Microsoft.AspNetCore.Mvc;
using Acies.Docs.Api.Models;
using Common.Controllers;
using Common.Attributes;
using Common.Models;

namespace Acies.Docs.Api.Controllers.V1.Documents.Outputs
{
    [ApiVersion("1.0")]
    [Route(nameof(Documents) + "/{documentId}/" + nameof(Outputs) + "/{outputId}/" + "[controller]")]
    [ApiController]
    public class AccessesController : CoreController
    {
        private readonly IDocumentService _documentService;
        private readonly TenantContext _tenantContext;
        private readonly IOutputService _outputService;

        public AccessesController(IDocumentService documentService, TenantContext tenantContext, IOutputService outputService)
        {
            _documentService = documentService;
            _tenantContext = tenantContext;
            _outputService = outputService;
        }

        [HttpGet]
        [Authorize(Permission.DocumentGet)]
        public async Task<Response<SignedUrlResource>> GetSignedOutputUrl(string documentId, string outputId)
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

                if (output is null || output.Status != Status.Succeeded) return ErrorResponse<SignedUrlResource>.ResourceNotFound();

                var request = _outputService.GetPreSignedUrlRequest(output.Name, r.Id, output.Id);
                var signedUrl = _outputService.GetPreSignedUrlResource(request);
                var resource = new SignedUrlResource()
                {
                    Expires = signedUrl.Expires,
                    Url = signedUrl.Url,
                };

                return new Response<SignedUrlResource>(resource);
            }
            else
            {
                return ErrorResponse<SignedUrlResource>.ResourceNotFound();
            }
        }
    }
}
