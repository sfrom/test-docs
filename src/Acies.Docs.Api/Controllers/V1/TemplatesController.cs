using Acies.Docs.Models;
using Microsoft.AspNetCore.Mvc;
using Acies.Docs.Api.Models;
using Common.Controllers;
using Common.Attributes;
using Common.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Acies.Docs.Api.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("[controller]")]
    [ApiController]
    public class TemplatesController : CoreController
    {
        private readonly ITemplateService _templateService;
        private readonly TenantContext _tenantContext;

        public TemplatesController(ITemplateService templateService, TenantContext tenantContext)
        {
            _templateService = templateService;
            _tenantContext = tenantContext;
        }

        [HttpGet]
        [Authorize(Permission.TemplateGetList)]
        public async Task<Response<IEnumerable<TemplateResource>>> Get()
        {
            if (Request.Query.Count > 0)
            {
                var d = new Dictionary<string, string>();
                foreach (var prop in Request.Query)
                {
                    d.Add(prop.Key, prop.Value);
                }
                var r = await _templateService.GetTemplatesByTagsAsync(d);
                if (r is { })
                {
                    var resource = r?.Select(c => c.MapTemplate(Request.Path));
                    return new Response<IEnumerable<TemplateResource>>(resource);
                }
                else
                {
                    return ErrorResponse<IEnumerable<TemplateResource>>.ResourceNotFound();
                }
            }
            return ErrorResponse<IEnumerable<TemplateResource>>.ResourceNotFound();
        }

        [HttpGet("{id}", Name = "GetTemplateEndpoint")]
        public async Task<Response<TemplateResource>> GetByIdAsync(string id)
        {
            var r = await _templateService.GetTemplateByKeyAsync(id);

            if (r is { })
            {
                var resource = r?.MapTemplate(Request.Path);
                return new Response<TemplateResource>(resource);
            }
            else
            {
                return ErrorResponse<TemplateResource>.ResourceNotFound();
            }
        }

        [HttpPost]
        [Authorize(Permission.TemplateCreate)]
        public async Task<Response<TemplateResource>> PostAsync([FromBody] TemplateCreateData data)
        {
            var r = await _templateService.CreateTemplateAsync(data);

            if (r is { })
            {
                var resource = r?.MapTemplate(Request.Path);
                return Responses.ResourceCreated<TemplateResource>(resource);
            }
            else
            {
                return ErrorResponse<TemplateResource>.ResourceNotFound();
            }
        }

        [HttpPut("{id}")]
        [Authorize(Permission.TemplateUpdate)]
        public async Task<Response<TemplateResource>> Put(string id, [FromBody] TemplateUpdateData data)
        {
            try
            {
                var r = await _templateService.UpdateTemplateAsync(id, data);
                if (r is { })
                {
                    var resource = r?.MapTemplate(Request.Path);
                    return Responses.ResourceUpdated<TemplateResource>(resource);
                }
                else
                {
                    return ErrorResponse<TemplateResource>.ResourceNotFound();
                }
            }
            catch (ArgumentException)
            {
                return ErrorResponse<TemplateResource>.ResourceNotFound();
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Permission.TemplateDelete)]
        public void Delete(int id)
        {
        }
    }
}