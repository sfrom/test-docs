using Acies.Docs.Api.Models;
using Acies.Docs.Models.Interfaces;
using Common.Controllers;
using Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Acies.Docs.Api.Controllers.V1.Templates
{
    [ApiVersion("1.0")]
    [Route(nameof(Templates) + "/{templateId}/[controller]")]
    [ApiController]
    public class AssetsController : CoreController
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
        }
        
        [Microsoft.AspNetCore.Mvc.HttpGet]
        [Common.Attributes.Authorize(Permission.AssetUpload)]
        public async Task<Response<SignedUrlResource>> GetSignedUriAsync(string templateId)
        {
            try
            {
                var response = await _assetService.GetPreSignedUrl(templateId);

                return response.Success 
                    ? new Response<SignedUrlResource>(response.MapSignedUrl())
                    : new ErrorResponse<SignedUrlResource>(response.ErrorMessage);
            }
            catch (Exception ex)
            {
                return new ErrorResponse<SignedUrlResource>("Could not generate signed uri");
            }
        }
    }
}