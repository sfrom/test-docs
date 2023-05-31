using Acies.Docs.Models;
using Microsoft.AspNetCore.Mvc;
using Acies.Docs.Api.Models;
using Common.Controllers;
using Common.Attributes;
using Common.Models;

namespace Acies.Docs.Api.Controllers.V1.Templates
{
    [ApiVersion("1.0")]
    [Route(nameof(Templates) + "/{templateId}/[controller]")]
    [ApiController]
    public class VersionsController : CoreController
    {
        private readonly ITemplateService _templateService;
        private readonly TenantContext _tenantContext;

        public VersionsController(ITemplateService templateService, TenantContext tenantContext)
        {
            _templateService = templateService;
            _tenantContext = tenantContext;
        }

        [HttpGet("{version}")]
        [Authorize(Permission.TemplateGet)]
        public async Task<Response<TemplateVersionResource>> GetByIdAsync(string templateId, int version)
        {
            TemplateVersion? r;
            if (version != 0)
            {
                r = await _templateService.GetTemplateVersionByKeyAndVersionAsync(templateId, version);
            }
            else
            {
                var template = await _templateService.GetTemplateByKeyAsync(templateId);
                r = await _templateService.GetTemplateVersionByKeyAndVersionAsync(template.Id, template.Version);
            }

            if (r is { })
            {
                var resource = r?.MapTemplateVersion(Request.Path);
                return new Response<TemplateVersionResource>(resource);
            }
            else
            {
                return ErrorResponse<TemplateVersionResource>.ResourceNotFound();
            }
        }
    }
}
