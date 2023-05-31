using Acies.Docs.Models.Models;

namespace Acies.Docs.Models.Interfaces;

public interface IAssetService
{
    Task<SignedUrlResponse> GetPreSignedUrl(string templateVersionId);
}