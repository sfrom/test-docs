using Acies.Docs.Models;
using Amazon.S3;

namespace Acies.Docs.Services.Amazon
{
    public class S3ReadOnlyStreamRepository : IReadOnlyStreamRepository
    {
        private readonly IAmazonS3 _s3Client;
        private readonly S3ReadOnlyStreamRepositoryOptions _options;

        public S3ReadOnlyStreamRepository(IAmazonS3 s3Client, S3ReadOnlyStreamRepositoryOptions options)
        {
            _s3Client = s3Client;
            _options = options;
        }

        public async Task<Stream> GetStreamAsync(string key)
        {
            var responce = await _s3Client.GetObjectAsync(_options.Bucket, key);
            return responce.ResponseStream;
        }
    }
}