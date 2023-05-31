using Amazon.S3;

namespace Acies.Docs.Api.Models
{
    public class SignedUrlResource
    {
        public string? Url { get; set; }
        public long Expires { get; set; }
        public HttpVerb MethodType { get; set; }
        public Dictionary<string, string>? RequiredHeaders { get; set; }
    }
}
