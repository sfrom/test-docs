using Acies.Docs.Models.Interfaces;
using Acies.Docs.Models.Models;
using Acies.Docs.Services.Repositories;
using Amazon.S3;
using Amazon.S3.Model;
using Common.Models;
using Logger.Interfaces;

namespace Acies.Docs.Services.Services;

public class AssetService : IAssetService
{
    private readonly TenantContext _context;
    private readonly ITemplateRepository _templateRepository;
    private readonly IAmazonS3 _s3;
    private readonly string _uploadBucket;
    private readonly bool _isLocalEnvironment;

    public AssetService(TenantContext context, ITemplateRepository templateRepository, IEnvironmentVariableProvider provider, IAmazonS3 s3)
    {
        _uploadBucket = provider.GetVariable("ASSETS_UPLOAD_BUCKET");
        _isLocalEnvironment = provider.GetOptionalVariable("AWS_EXECUTION_ENV") is null;
        _context = context;
        _templateRepository = templateRepository;
        _s3 = s3;
    }
    
    public async Task<SignedUrlResponse> GetPreSignedUrl(string templateVersionId)
    {
        var response = new SignedUrlResponse();

        if (!_isLocalEnvironment && await _templateRepository.GetAsync(templateVersionId) is null)
        {
            response.ErrorMessage = $"Template version with id {templateVersionId} does not exist";
            return await Task.FromResult(response);
        }
        
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = $@"{_uploadBucket}/accounts/{_context.AccountId}/templates/{templateVersionId}/assets",
                Key = "assets.zip",
                Expires = DateTime.Now.AddMinutes(5),
                Verb = HttpVerb.PUT
            };
            
            response.Url = _s3.GetPreSignedURL(request);
            response.Method = request.Verb;
            response.Expires = ((DateTimeOffset)request.Expires).ToUnixTimeSeconds();
            response.Success = true;
        }
        catch (Exception e)
        {
            response.ErrorMessage = e.Message;
        }
        
        return await Task.FromResult(response);
    }
}