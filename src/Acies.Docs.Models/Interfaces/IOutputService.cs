using Amazon.S3.Model;

namespace Acies.Docs.Models
{
    public interface IOutputService
    {
        public GetPreSignedUrlRequest GetPreSignedUrlRequest(string fileName, string documentId, string outputId);
        public SignedUrl GetPreSignedUrlResource(GetPreSignedUrlRequest request);
    }
}