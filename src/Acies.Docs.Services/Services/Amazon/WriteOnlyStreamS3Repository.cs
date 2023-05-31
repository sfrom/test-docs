using Acies.Docs.Models;
using Amazon.S3;
using Amazon.S3.Model;

namespace Acies.Docs.Services.Amazon
{
    public class S3WriteOnlyStreamRepository : IWriteOnlyStreamRepository
    {
        private readonly IAmazonS3 _s3Client;
        private readonly S3WriteOnlyStreamRepositoryOptions _options;

        public S3WriteOnlyStreamRepository(IAmazonS3 s3Client, S3WriteOnlyStreamRepositoryOptions options)
        {
            _s3Client = s3Client;
            _options = options;
        }

        public async Task WriteAsync(Stream stream, string key)
        {
            var request = new PutObjectRequest
            {
                BucketName = _options.Bucket,
                Key = key,
                InputStream = stream,
            };
            await _s3Client.PutObjectAsync(request);
        }
    }
}
