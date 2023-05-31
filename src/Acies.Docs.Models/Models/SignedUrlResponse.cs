using Amazon.S3;

namespace Acies.Docs.Models.Models;

public class SignedUrlResponse
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public HttpVerb Method { get; set; }
    public long Expires { get; set; }
    public string Url { get; set; }
}