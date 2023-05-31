using Acies.Docs.Models;
using Amazon.S3;
using Amazon.S3.Model;
using Common.Models;
using DatabaseContext.Models;
using Logger.Interfaces;

namespace Acies.Docs.Services
{
    public class OutputService : IOutputService
    {
        private readonly TenantContext _tenantContext;
        private readonly IAmazonS3 _s3Client;
        public string AccountIdKey = "x-amz-meta-accountid";

        public OutputService(TenantContext tenantContext, IAmazonS3 s3Client)
        {
            _tenantContext = tenantContext;
            _s3Client = s3Client;
        }

        public GetPreSignedUrlRequest GetPreSignedUrlRequest(string fileName, string documentId, string outputId)
        {
            var key = "accounts/" + _tenantContext.AccountId + "/documents/" + documentId + "/outputs/" + outputId + "/" + fileName;
            var expire = DateTime.Now.AddMinutes(60);
            var method = HttpVerb.GET;

            var req = new GetPreSignedUrlRequest
            {
                BucketName = Environment.GetEnvironmentVariable("RESOURCE_BUCKET"),
                Key = key,
                Expires = expire,
                Verb = method
            };

            return req;
        }

        public SignedUrl GetPreSignedUrlResource(GetPreSignedUrlRequest request)
        {
            return new SignedUrl()
            {
                Url = _s3Client.GetPreSignedURL(request),
                Key = request.Key,
                Expires = ((DateTimeOffset)request.Expires).ToUnixTimeSeconds(),
                MethodType = $"{request.Verb}"
            };
        }
    }
}
