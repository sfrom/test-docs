using Acies.Docs.Models;
using Microsoft.AspNetCore.Mvc;
using Acies.Docs.Api.Models;
using Common.Controllers;
using Common.Attributes;
using Common.Models;

namespace Acies.Docs.Api.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class DocumentsController : CoreController
    {
        private readonly IDocumentService _documentService;
        private readonly TenantContext _tenantContext;

        public DocumentsController(IDocumentService documentService, TenantContext tenantContext)
        {
            _documentService = documentService;
            _tenantContext = tenantContext;
        }

        [HttpGet]
        [Authorize(Permission.DocumentGetList)]
        public async Task<Response<IEnumerable<DocumentResource>>> GetAsync()
        {
            if (Request.Query.Count > 0)
            {
                var tags = new Dictionary<string, string>();
                foreach (var prop in Request.Query)
                {
                    tags.Add(prop.Key, prop.Value);
                }
                var r = await _documentService.GetDocumentsByTagsAsync(tags);

                if (r is { })
                {
                    var resource = r?.Select(c => c.MapDocument(Request.Path));
                    return new Response<IEnumerable<DocumentResource>>(resource);
                }
                else
                {
                    return ErrorResponse<IEnumerable<DocumentResource>>.ResourceNotFound();
                }
            }
            return ErrorResponse<IEnumerable<DocumentResource>>.ResourceNotFound();
        }

        [HttpGet("{id}", Name = "GetDocumentEndpoint")]
        [Authorize(Permission.DocumentGet)]
        public async Task<Response<DocumentResource>> GetByIdAsync(string id)
        {
            var r = await _documentService.GetDocumentByKeyAsync(id);

            if (r is { })
            {
                var resource = r?.MapDocument(Request.Path);
                return new Response<DocumentResource>(resource);
            }
            else
            {
                return ErrorResponse<DocumentResource>.ResourceNotFound();
            }
        }

        [HttpPost]
        [Authorize(Permission.DocumentCreate)]
        public async Task<Response<DocumentResource>> PostAsync([FromBody] DocumentCreateData data)
        {
            var r = await _documentService.CreateDocumentAsync(data);
            if (r.Success)
            {
                if (r is { })
                {
                    var resource = r.Document?.MapDocument(Request.Path);
                    return Responses.ResourceCreated<DocumentResource>(resource);
                }
                else
                {
                    return ErrorResponse<DocumentResource>.ResourceNotFound();
                }
            }
            else
            {
                Console.WriteLine(r.ErrorMessage);
                return new ErrorResponse<DocumentResource>(r.ErrorMessage);
            }

        }

        [HttpPut("{id}")]
        [Authorize(Permission.DocumentUpdate)]
        public async Task<Response<DocumentResource>> Put(string id, [FromBody] DocumentUpdateData data)
        {
            try
            {
                var r = await _documentService.UpdateDocumentAsync(id, data);
                if (r.Success)
                {
                    if (r.Document is { })
                    {
                        var resource = r.Document?.MapDocument(Request.Path);
                        return Responses.ResourceUpdated<DocumentResource>(resource);
                    }
                    else
                    {
                        return ErrorResponse<DocumentResource>.ResourceNotFound();
                    }
                }
                else
                {
                    Console.WriteLine(r.ErrorMessage);
                    return new ErrorResponse<DocumentResource>(r.ErrorMessage);
                }
            }
            catch (ArgumentException)
            {
                return ErrorResponse<DocumentResource>.ResourceNotFound();
            }
        }
    }
}
